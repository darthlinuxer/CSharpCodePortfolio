using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;

/// <summary>
/// Maps the persisted integration outbox.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    private static readonly DateTime NoTimestamp = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

    private static readonly ValueConverter<Timestamp, DateTime> TimestampConverter = new(
        timestamp => timestamp.Value,
        value => Timestamp.FromTrustedUtc(value));

    private static readonly ValueConverter<Option<Timestamp>, DateTime> OptionalTimestampConverter = new(
        timestamp => TimestampToProvider(timestamp),
        value => TimestampFromProvider(value));

    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Type)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(message => message.Payload)
            .IsRequired();

        builder.Property(message => message.OccurredAtUtc)
            .HasConversion(TimestampConverter)
            .IsRequired();

        builder.Property(message => message.ProcessedAtUtc)
            .HasConversion(OptionalTimestampConverter);

        builder.Property(message => message.AttemptCount)
            .IsRequired();

        builder.Property(message => message.LastAttemptedAtUtc)
            .HasConversion(OptionalTimestampConverter);

        builder.Property(message => message.LastError)
            .HasMaxLength(1000);
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
