using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Application.Abstractions;

public interface IOrderEventPublisher
{
    Task PublishAsync(OrderCreatedEvent @event, CancellationToken cancellationToken);
}
