using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Worker.Application.Abstractions;
using OrderFlow.Worker.Domain.Orders;

namespace OrderFlow.Worker.Application.Orders;

public sealed class OrderProcessor(
    IOrderRepository repository,
    TimeProvider timeProvider,
    IOptions<OrderProcessingOptions> options,
    ILogger<OrderProcessor> logger) : IOrderProcessor
{
    public async Task ProcessAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Pedido {OrderId} não encontrado. Mensagem descartada.", orderId);
            return;
        }

        if (order.Status == OrderStatus.Finalizado)
        {
            logger.LogInformation("Pedido {OrderId} já está finalizado. Nada a fazer.", orderId);
            return;
        }

        try
        {
            if (order.Status == OrderStatus.Pendente)
            {
                order.StartProcessing();
                await repository.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Pedido {OrderId} em processamento.", orderId);
            }

            await Task.Delay(options.Value.ProcessingDelay, timeProvider, cancellationToken);

            order.Finish(timeProvider.GetUtcNow());
            await repository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Pedido {OrderId} finalizado.", orderId);
        }
        catch (ConcurrencyConflictException)
        {
            logger.LogInformation("Pedido {OrderId} já foi atualizado por outro consumidor. Processamento ignorado.", orderId);
        }
    }
}
