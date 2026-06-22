using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Persistence.Outbox;

internal sealed record OutboxEventEnvelope(
    string EventName,
    int EventVersion,
    string AggregateType,
    string AggregateId,
    string Payload,
    Timestamp OccurredOnUtc);
