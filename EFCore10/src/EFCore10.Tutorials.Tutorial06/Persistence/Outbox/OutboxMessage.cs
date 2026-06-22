using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Persistence.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        string eventName,
        int eventVersion,
        string aggregateType,
        string aggregateId,
        string payload,
        Timestamp occurredOnUtc)
    {
        Id = id;
        EventName = eventName;
        EventVersion = eventVersion;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; private set; }

    public string EventName { get; private set; } = string.Empty;

    public int EventVersion { get; private set; }

    public string AggregateType { get; private set; } = string.Empty;

    public string AggregateId { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public Timestamp OccurredOnUtc { get; private set; }

    public string Status { get; private set; } = "Pending";

    public int RetryCount { get; private set; }

    public Timestamp? NextAttemptOnUtc { get; private set; }

    public Timestamp? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        var envelope = OutboxEventMapper.Map(domainEvent);

        return new OutboxMessage(
            Guid.CreateVersion7(),
            envelope.EventName,
            envelope.EventVersion,
            envelope.AggregateType,
            envelope.AggregateId,
            envelope.Payload,
            envelope.OccurredOnUtc);
    }
}
