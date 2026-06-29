using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence.ConfigurationMappings;

/// <summary>
/// Maps the DDD aggregate directly, using EF Core complex properties for value objects.
/// </summary>
public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    private static readonly ValueConverter<Timestamp?, DateTime?> TimestampConverter = new(
        timestamp => timestamp == null ? null : timestamp.Value,
        value => value == null ? null : Timestamp.FromTrustedUtc(value.Value));

    /// <summary>
    /// Configures the aggregate table without introducing a persistence-only record type.
    /// </summary>
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Ignore(user => user.DomainEvents);
        builder.Ignore(user => user.CreatedAt);
        builder.Ignore(user => user.CreatedBy);
        builder.Ignore(user => user.LastModified);
        builder.Ignore(user => user.LastModifiedBy);
        builder.Ignore(user => user.PhoneNumber);

        builder.Property<Timestamp?>("_createdAt")
            .HasConversion(TimestampConverter)
            .HasColumnName("CreatedAtUtc");

        builder.Property<Timestamp?>("_lastModified")
            .HasConversion(TimestampConverter)
            .HasColumnName("LastModifiedAtUtc");

        builder.ComplexProperty(user => user.Name, name =>
        {
            name.Property(value => value.Value)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Property(user => user.Document)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(user => user.Document)
            .IsUnique();

        builder.Property(user => user.Email)
            .HasConversion(
                email => email.Value,
                value => Email.FromTrustedValue(value))
            .HasColumnName("Email")
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.ComplexProperty(user => user.PhoneNumberValue, phone =>
        {
            phone.Property(value => value.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(15);
        });
    }
}
