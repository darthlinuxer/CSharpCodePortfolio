using EFCore10.Tutorials.Tutorial06.Persistence.Outbox;
using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.EventName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.EventVersion)
            .IsRequired();

        builder.Property(message => message.AggregateType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(message => message.AggregateId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(message => message.Payload)
            .IsRequired();

        builder.Property(message => message.OccurredOnUtc)
            .HasConversion(timestamp => timestamp.Value, value => Timestamp.FromDatabase(value))
            .IsRequired();

        builder.Property(message => message.Status)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(message => message.RetryCount)
            .IsRequired();

        builder.Property(message => message.NextAttemptOnUtc)
            .HasConversion(
                timestamp => timestamp.HasValue ? timestamp.Value.Value : (DateTime?)null,
                value => value.HasValue ? Timestamp.FromDatabase(value.Value) : null);

        builder.Property(message => message.ProcessedOnUtc)
            .HasConversion(
                timestamp => timestamp.HasValue ? timestamp.Value.Value : (DateTime?)null,
                value => value.HasValue ? Timestamp.FromDatabase(value.Value) : null);

        builder.Property(message => message.Error);

        builder.HasIndex(message => new { message.Status, message.NextAttemptOnUtc });
        builder.HasIndex(message => new { message.AggregateType, message.AggregateId });
    }
}
