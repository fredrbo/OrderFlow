using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Infrastructure.Persistence;

public sealed class OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderFlowDbContext).Assembly);
}
