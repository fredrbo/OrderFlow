using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderFlow.Worker.Application.Orders;
using OrderFlow.Worker.Application.Orders.Messages;
using OrderFlow.Worker.Messaging;
using OrderFlow.Worker.Resilience;
using Polly;
using Polly.Registry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace OrderFlow.Worker.Consumers;

public sealed class OrderCreatedConsumer(
    IServiceScopeFactory scopeFactory,
    ResiliencePipelineProvider<string> pipelineProvider,
    IOptions<RabbitMqOptions> options,
    ILogger<OrderCreatedConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly RabbitMqOptions _options = options.Value;
    private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline(OrderProcessingResilience.PipelineName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await ConnectAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await DeclareTopologyAsync(channel, stoppingToken);
        await channel.BasicQosAsync(0, _options.MaxConcurrentMessages, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, delivery) => HandleAsync(channel, delivery, stoppingToken);

        await channel.BasicConsumeAsync(_options.OrderCreatedQueue, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Consumindo a fila {Queue}.", _options.OrderCreatedQueue);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Encerrando o consumo da fila {Queue}.", _options.OrderCreatedQueue);
        }
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken stoppingToken)
    {
        OrderCreatedMessage? message;

        try
        {
            message = JsonSerializer.Deserialize<OrderCreatedMessage>(delivery.Body.Span, SerializerOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Mensagem inválida enviada para a DLQ (delivery tag {DeliveryTag}).", delivery.DeliveryTag);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, requeue: false, CancellationToken.None);
            return;
        }

        if (message is null || message.Id == Guid.Empty)
        {
            logger.LogError("Mensagem sem id de pedido enviada para a DLQ (delivery tag {DeliveryTag}).", delivery.DeliveryTag);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, requeue: false, CancellationToken.None);
            return;
        }

        var context = ResilienceContextPool.Shared.Get(stoppingToken);
        context.Properties.Set(OrderProcessingResilience.OrderIdKey, message.Id);

        try
        {
            await _pipeline.ExecuteAsync(async ctx =>
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOrderProcessor>();
                await processor.ProcessAsync(message.Id, ctx.CancellationToken);
            }, context);

            await channel.BasicAckAsync(delivery.DeliveryTag, false, CancellationToken.None);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Processamento do pedido {OrderId} interrompido; a mensagem voltará para a fila.", message.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha definitiva ao processar o pedido {OrderId}. Mensagem enviada para a DLQ.", message.Id);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, requeue: false, CancellationToken.None);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    private async Task<IConnection> ConnectAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            ClientProvidedName = "orderflow-worker",
            ConsumerDispatchConcurrency = _options.MaxConcurrentMessages
        };

        while (true)
        {
            try
            {
                return await factory.CreateConnectionAsync(stoppingToken);
            }
            catch (BrokerUnreachableException ex)
            {
                logger.LogWarning(ex, "RabbitMQ indisponível. Nova tentativa em {Delay}.", _options.ConnectionRetryDelay);
                await Task.Delay(_options.ConnectionRetryDelay, stoppingToken);
            }
        }
    }

    private async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            _options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            _options.OrderCreatedDeadLetterQueue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            _options.OrderCreatedQueue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = string.Empty,
                ["x-dead-letter-routing-key"] = _options.OrderCreatedDeadLetterQueue
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            _options.OrderCreatedQueue, _options.Exchange, _options.OrderCreatedRoutingKey,
            cancellationToken: cancellationToken);
    }
}
