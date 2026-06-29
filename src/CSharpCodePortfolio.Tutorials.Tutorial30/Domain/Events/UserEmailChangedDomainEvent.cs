namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Domain event raised when a registered user's required email changes.
/// </summary>
public sealed record UserEmailChangedDomainEvent(
    Guid UserId,
    Email PreviousEmail,
    Email NewEmail,
    Timestamp OccurredAtUtc) : IDomainEvent
{
    /// <summary>
    /// Gets the stable typed event name.
    /// </summary>
    public DomainEventType EventType => DomainEventTypes.UserEmailChanged;
}
