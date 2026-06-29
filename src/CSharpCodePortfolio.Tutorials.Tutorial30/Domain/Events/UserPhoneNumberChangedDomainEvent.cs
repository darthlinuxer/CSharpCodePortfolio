using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Domain event raised when a registered user's optional phone changes.
/// </summary>
public sealed record UserPhoneNumberChangedDomainEvent(
    Guid UserId,
    Option<PhoneNumber> PreviousPhoneNumber,
    Option<PhoneNumber> NewPhoneNumber,
    Timestamp OccurredAtUtc) : IDomainEvent
{
    /// <summary>
    /// Gets the stable typed event name.
    /// </summary>
    public DomainEventType EventType => DomainEventTypes.UserPhoneNumberChanged;
}
