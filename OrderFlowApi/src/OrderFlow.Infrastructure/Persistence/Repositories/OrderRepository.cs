using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(OrderFlowDbContext dbContext) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Historico.OrderBy(h => h.DataAlteracao))
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.DataCriacao)
            .ToListAsync(cancellationToken);
}
