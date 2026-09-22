using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Orders.Events;
using RabbitMQ.Client;

namespace OrderFlow.Infrastructure.Messaging;

internal sealed class RabbitMqOrderEventPublisher(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqOrderEventPublisher> logger) : IOrderEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() }
    };

    // Com confirmações habilitadas, BasicPublishAsync só retorna após o broker confirmar o recebimento.
    private static readonly CreateChannelOptions ChannelOptions = new(
        publisherConfirmationsEnabled: true,
        publisherConfirmationTrackingEnabled: true);

    private readonly RabbitMqOptions _options = options.Value;

    public async Task PublishAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        var conn = await connection.GetAsync(cancellationToken);
        await using var channel = await conn.CreateChannelAsync(ChannelOptions, cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event, SerializerOptions);
        var properties = new BasicProperties
        {
            MessageId = @event.Id.ToString(),
            ContentType = "application/json",
            Type = _options.OrderCreatedRoutingKey,
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(@event.DataCriacao.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            _options.Exchange, _options.OrderCreatedRoutingKey, mandatory: false,
            properties, body, cancellationToken);

        logger.LogInformation("Evento {RoutingKey} publicado para o pedido {OrderId}",
            _options.OrderCreatedRoutingKey, @event.Id);
    }
}
