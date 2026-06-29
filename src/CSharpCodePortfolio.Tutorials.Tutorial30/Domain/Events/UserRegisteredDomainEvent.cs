namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Domain event raised when the registration aggregate is created successfully.
/// </summary>
public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Document,
    Email Email,
    Timestamp OccurredAtUtc) : IDomainEvent
{
    /// <summary>
    /// Gets the stable event type without repeating the literal across the codebase.
    /// </summary>
    public DomainEventType EventType => DomainEventTypes.UserRegistered;
}
