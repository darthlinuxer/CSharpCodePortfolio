using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Persistence.ConfigurationMappings;

/// <summary>
/// Maps OrderLine as an entity owned by Order.
/// </summary>
public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");
        builder.HasKey(line => line.Id);

        builder.Property(line => line.Sku)
            .HasConversion(OrderConfiguration.SkuValueConverter)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(line => line.Quantity)
            .HasConversion(OrderConfiguration.QuantityValueConverter)
            .IsRequired();

        builder.Property(line => line.UnitPrice)
            .HasConversion(OrderConfiguration.MoneyValueConverter)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Ignore(line => line.LineTotal);
    }
}
