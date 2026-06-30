using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence.ConfigurationMappings;

/// <summary>
/// Maps the DDD aggregate directly, using EF Core complex properties for value objects.
/// </summary>
public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    private static readonly DateTime NoTimestamp = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

    private static readonly ValueConverter<Option<Timestamp>, DateTime> TimestampConverter = new(
        timestamp => TimestampToProvider(timestamp),
        value => TimestampFromProvider(value));

    private static readonly ValueConverter<PersonName, string> PersonNameConverter = new(
        name => name.Value,
        value => new PersonName(value));

    private static readonly ValueConverter<Email, string> EmailConverter = new(
        email => email.Value,
        value => new Email(value));

    private static readonly ValueConverter<Option<PhoneNumber>, string> PhoneNumberConverter = new(
        phone => PhoneToProvider(phone),
        value => PhoneFromProvider(value));

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
        builder.Ignore(user => user.LastModified);
        builder.Ignore(user => user.PhoneNumber);

        builder.Property<Option<Timestamp>>("_createdAt")
            .HasConversion(TimestampConverter)
            .HasColumnName("CreatedAtUtc");

        builder.Property<Option<Timestamp>>("_lastModified")
            .HasConversion(TimestampConverter)
            .HasColumnName("LastModifiedAtUtc");

        builder.Property(user => user.Name)
            .HasConversion(PersonNameConverter)
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasConversion(EmailConverter)
            .HasColumnName("Email")
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PhoneNumberValue)
            .HasConversion(PhoneNumberConverter)
            .HasColumnName("PhoneNumber")
            .HasMaxLength(15);
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

    private static string PhoneToProvider(Option<PhoneNumber> phone)
    {
        foreach (var value in phone)
        {
            return value.Value;
        }

        return string.Empty;
    }

    private static Option<PhoneNumber> PhoneFromProvider(string value) =>
        string.IsNullOrWhiteSpace(value) ? None : Some(new PhoneNumber(value));
}
