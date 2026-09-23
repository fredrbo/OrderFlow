using Microsoft.EntityFrameworkCore;
using OrderFlow.Worker.Application.Abstractions;
using OrderFlow.Worker.Domain.Orders;

namespace OrderFlow.Worker.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(OrderFlowDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex);
        }
    }
}
