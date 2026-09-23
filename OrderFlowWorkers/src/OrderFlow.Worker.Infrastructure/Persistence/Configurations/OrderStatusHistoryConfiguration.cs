using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Worker.Domain.Orders;

namespace OrderFlow.Worker.Infrastructure.Persistence.Configurations;

internal sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(h => h.OrderId).HasColumnName("order_id");
        builder.Property(h => h.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.DataAlteracao).HasColumnName("data_alteracao");
    }
}
