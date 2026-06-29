namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Domain event raised when a registered user's name changes.
/// </summary>
public sealed record UserNameChangedDomainEvent(
    Guid UserId,
    PersonName PreviousName,
    PersonName NewName,
    Timestamp OccurredAtUtc) : IDomainEvent
{
    /// <summary>
    /// Gets the stable typed event name.
    /// </summary>
    public DomainEventType EventType => DomainEventTypes.UserNameChanged;
}
