using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Persistence.ConfigurationMappings;

/// <summary>
/// Maps the Order aggregate and its owned OrderLine entities.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    private static readonly DateTime NoTimestamp = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

    private static readonly ValueConverter<OrderId, Guid> OrderIdConverter = new(
        id => id.Value,
        value => new OrderId(value));

    private static readonly ValueConverter<CustomerId, Guid> CustomerIdConverter = new(
        id => id.Value,
        value => new CustomerId(value));

    private static readonly ValueConverter<Sku, string> SkuConverter = new(
        sku => sku.Value,
        value => new Sku(value));

    private static readonly ValueConverter<Quantity, int> QuantityConverter = new(
        quantity => quantity.Value,
        value => new Quantity(value));

    private static readonly ValueConverter<Money, decimal> MoneyConverter = new(
        money => money.Value,
        value => new Money(value));

    private static readonly ValueConverter<Option<Timestamp>, DateTime> TimestampConverter = new(
        timestamp => TimestampToProvider(timestamp),
        value => TimestampFromProvider(value));

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id)
            .HasConversion(OrderIdConverter)
            .ValueGeneratedNever();

        builder.Property(order => order.CustomerId)
            .HasConversion(CustomerIdConverter)
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Ignore(order => order.DomainEvents);
        builder.Ignore(order => order.CreatedAt);
        builder.Ignore(order => order.LastModified);
        builder.Ignore(order => order.Total);

        builder.Property<Option<Timestamp>>("_createdAt")
            .HasConversion(TimestampConverter)
            .HasColumnName("CreatedAtUtc");

        builder.Property<Option<Timestamp>>("_lastModified")
            .HasConversion(TimestampConverter)
            .HasColumnName("LastModifiedAtUtc");

        builder.HasMany(order => order.Lines)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static DateTime TimestampToProvider(Option<Timestamp> timestamp)
    {
        foreach (var value in timestamp)
        {
            return value.Value;
        }

        return NoTimestamp;
    }

    private static Option<Timestamp> TimestampFromProvider(DateTime value) =>
        value == NoTimestamp ? None : Some(Timestamp.FromTrustedUtc(value));

    internal static ValueConverter<Sku, string> SkuValueConverter => SkuConverter;

    internal static ValueConverter<Quantity, int> QuantityValueConverter => QuantityConverter;

    internal static ValueConverter<Money, decimal> MoneyValueConverter => MoneyConverter;
}
