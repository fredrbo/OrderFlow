using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Worker.Domain.Orders;

namespace OrderFlow.Worker.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(o => o.Cliente).HasColumnName("cliente").HasMaxLength(200).IsRequired();
        builder.Property(o => o.Produto).HasColumnName("produto").HasMaxLength(200).IsRequired();
        builder.Property(o => o.Valor).HasColumnName("valor").HasPrecision(18, 2);
        builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.DataCriacao).HasColumnName("data_criacao");
        builder.Property(o => o.DataFinalizacao).HasColumnName("data_finalizacao");
        builder.Property<uint>("Version").IsRowVersion();
    }
}
