using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Orders.Dtos;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders;

public sealed class OrderService(
    IOrderRepository repository,
    IOrderEventOutbox outbox,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IOrderService
{
    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = Order.Create(request.Cliente, request.Produto, request.Valor, timeProvider.GetUtcNow());

        repository.Add(order);
        outbox.Add(OrderCreatedEvent.From(order));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderResponse.From(order);
    }

    public async Task<OrderDetailsResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : OrderDetailsResponse.From(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var orders = await repository.ListAsync(cancellationToken);
        return orders.Select(OrderResponse.From).ToList();
    }
}
