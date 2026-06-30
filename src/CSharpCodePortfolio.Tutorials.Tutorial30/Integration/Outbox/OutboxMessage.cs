using System.Text.Json;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;

/// <summary>
/// Persisted integration event envelope for the tutorial outbox.
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    private OutboxMessage(Guid id, string type, string payload, Timestamp occurredAtUtc)
    {
        Id = id;
        Type = type;
        Payload = payload;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Payload { get; private set; }

    public Timestamp OccurredAtUtc { get; private set; }

    public Option<Timestamp> ProcessedAtUtc { get; private set; } = None;

    public int AttemptCount { get; private set; }

    public Option<Timestamp> LastAttemptedAtUtc { get; private set; } = None;

    public string? LastError { get; private set; }

    public static OutboxMessage From(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var payload = JsonSerializer.Serialize((object)integrationEvent, integrationEvent.GetType());
        return new OutboxMessage(integrationEvent.Id, integrationEvent.Type, payload, integrationEvent.OccurredAtUtc);
    }

    public TIntegrationEvent Deserialize<TIntegrationEvent>()
        where TIntegrationEvent : IIntegrationEvent
    {
        return JsonSerializer.Deserialize<TIntegrationEvent>(Payload)
            ?? throw new InvalidOperationException($"Could not deserialize outbox message '{Id}'.");
    }

    public void MarkProcessed(Timestamp processedAtUtc)
    {
        LastAttemptedAtUtc = Some(processedAtUtc);
        ProcessedAtUtc = Some(processedAtUtc);
        LastError = null;
    }

    public void RecordFailure(Timestamp attemptedAtUtc, string error)
    {
        AttemptCount++;
        LastAttemptedAtUtc = Some(attemptedAtUtc);
        LastError = error;
    }
}
