using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Abstractions;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken);
}
