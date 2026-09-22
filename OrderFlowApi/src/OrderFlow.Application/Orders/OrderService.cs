using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Orders.Dtos;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders;

public sealed class OrderService(
    IOrderRepository repository,
    IOrderEventPublisher publisher,
    TimeProvider timeProvider) : IOrderService
{
    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = Order.Create(request.Cliente, request.Produto, request.Valor, timeProvider.GetUtcNow());

        await repository.AddAsync(order, cancellationToken);
        await publisher.PublishAsync(OrderCreatedEvent.From(order), cancellationToken);

        return OrderResponse.From(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : OrderResponse.From(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var orders = await repository.ListAsync(cancellationToken);
        return orders.Select(OrderResponse.From).ToList();
    }
}
