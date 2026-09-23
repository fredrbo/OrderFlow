namespace OrderFlow.Worker.Application.Orders;

public interface IOrderProcessor
{
    Task ProcessAsync(Guid orderId, CancellationToken cancellationToken);
}
