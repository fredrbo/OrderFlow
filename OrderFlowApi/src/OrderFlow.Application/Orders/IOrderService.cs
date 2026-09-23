using OrderFlow.Application.Orders.Dtos;

namespace OrderFlow.Application.Orders;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDetailsResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderResponse>> ListAsync(CancellationToken cancellationToken);
}
