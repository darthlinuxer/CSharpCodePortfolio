using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;

/// <summary>
/// Base record for domain facts that share a typed event name and UTC occurrence time.
/// </summary>
public abstract record DomainEvent(
    DomainEventType EventType,
    Timestamp OccurredAtUtc) : IDomainEvent;
