using OrderFlow.Worker.Domain.Orders;

namespace OrderFlow.Worker.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
