using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Persistence.ConfigurationMappings;

/// <summary>
/// Maps Ordering's local customer projection.
/// </summary>
public sealed class CustomerDirectoryConfiguration : IEntityTypeConfiguration<CustomerDirectoryEntry>
{
    private static readonly ValueConverter<CustomerId, Guid> CustomerIdConverter = new(
        id => id.Value,
        value => new CustomerId(value));

    public void Configure(EntityTypeBuilder<CustomerDirectoryEntry> builder)
    {
        builder.ToTable("OrderingCustomers");
        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Id)
            .HasConversion(CustomerIdConverter)
            .ValueGeneratedNever();

        builder.Property(customer => customer.Email)
            .HasMaxLength(320)
            .IsRequired();
    }
}
