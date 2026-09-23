using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Domain.Orders;
using OrderFlow.Infrastructure.Messaging.Outbox;

namespace OrderFlow.Infrastructure.Persistence;

public sealed class OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options, OutboxSignal outboxSignal)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var hasNewMessages = ChangeTracker.Entries<OutboxMessage>().Any(e => e.State == EntityState.Added);

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        if (hasNewMessages && Database.CurrentTransaction is null)
            outboxSignal.Notify();

        return result;
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderFlowDbContext).Assembly);
}
