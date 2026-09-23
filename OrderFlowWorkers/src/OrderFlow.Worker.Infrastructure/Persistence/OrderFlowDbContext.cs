using Microsoft.EntityFrameworkCore;
using OrderFlow.Worker.Domain.Orders;

namespace OrderFlow.Worker.Infrastructure.Persistence;

public sealed class OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderFlowDbContext).Assembly);
}
