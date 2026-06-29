namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Marker contract for facts raised by domain behavior inside the aggregate.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets a stable event type constant used by integrations and tests.
    /// </summary>
    DomainEventType EventType { get; }

    /// <summary>
    /// Gets when the fact occurred in UTC.
    /// </summary>
    Timestamp OccurredAtUtc { get; }
}
