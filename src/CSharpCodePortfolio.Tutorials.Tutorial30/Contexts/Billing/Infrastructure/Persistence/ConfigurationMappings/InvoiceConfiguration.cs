using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Infrastructure.Persistence.ConfigurationMappings;

/// <summary>
/// Maps Billing invoices with local value objects.
/// </summary>
public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    private static readonly DateTime NoTimestamp = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

    private static readonly ValueConverter<InvoiceId, Guid> InvoiceIdConverter = new(
        id => id.Value,
        value => new InvoiceId(value));

    private static readonly ValueConverter<BilledOrderId, Guid> BilledOrderIdConverter = new(
        id => id.Value,
        value => new BilledOrderId(value));

    private static readonly ValueConverter<BillingCustomerId, Guid> BillingCustomerIdConverter = new(
        id => id.Value,
        value => new BillingCustomerId(value));

    private static readonly ValueConverter<InvoiceAmount, decimal> InvoiceAmountConverter = new(
        amount => amount.Value,
        value => new InvoiceAmount(value));

    private static readonly ValueConverter<Option<Timestamp>, DateTime> TimestampConverter = new(
        timestamp => TimestampToProvider(timestamp),
        value => TimestampFromProvider(value));

    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.Id)
            .HasConversion(InvoiceIdConverter)
            .ValueGeneratedNever();

        builder.Property(invoice => invoice.OrderId)
            .HasConversion(BilledOrderIdConverter)
            .IsRequired();

        builder.Property(invoice => invoice.CustomerId)
            .HasConversion(BillingCustomerIdConverter)
            .IsRequired();

        builder.Property(invoice => invoice.Amount)
            .HasConversion(InvoiceAmountConverter)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(invoice => invoice.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(invoice => invoice.SourceIntegrationEventId)
            .IsRequired();

        builder.HasIndex(invoice => invoice.SourceIntegrationEventId)
            .IsUnique();

        builder.Ignore(invoice => invoice.DomainEvents);
        builder.Ignore(invoice => invoice.CreatedAt);
        builder.Ignore(invoice => invoice.LastModified);

        builder.Property<Option<Timestamp>>("_createdAt")
            .HasConversion(TimestampConverter)
            .HasColumnName("CreatedAtUtc");

        builder.Property<Option<Timestamp>>("_lastModified")
            .HasConversion(TimestampConverter)
            .HasColumnName("LastModifiedAtUtc");
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
}
