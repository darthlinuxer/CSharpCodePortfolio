namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Domain event raised when the registration aggregate is created successfully.
/// </summary>
/// <remarks>
/// The legacy <c>Document</c> payload has been retired alongside
/// <c>UserAccount.Document</c>: PF vs PJ modelling is deferred to a dedicated
/// bounded context and is not part of the abstract registration flow.
/// </remarks>
public sealed record UserAccountRegisteredDomainEvent(
    Guid UserId,
    Email Email,
    Timestamp OccurredAtUtc) : IDomainEvent
{
    /// <summary>
    /// Gets the stable event type without repeating the literal across the codebase.
    /// </summary>
    public DomainEventType EventType => UserAccountDomainEventTypes.Registered;
}
