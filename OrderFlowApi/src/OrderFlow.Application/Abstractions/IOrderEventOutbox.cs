using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Application.Abstractions;

public interface IOrderEventOutbox
{
    void Add(OrderCreatedEvent @event);
}
