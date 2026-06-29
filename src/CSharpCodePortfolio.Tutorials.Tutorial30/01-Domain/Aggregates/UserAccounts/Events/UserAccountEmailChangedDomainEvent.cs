using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Events;

/// <summary>
/// Domain event raised when a registered user's required email changes.
/// </summary>
public sealed record UserAccountEmailChangedDomainEvent(
    Guid UserId,
    Email PreviousEmail,
    Email NewEmail,
    Timestamp OccurredAtUtc)
    : DomainEvent(UserAccountDomainEventTypes.EmailChanged, OccurredAtUtc);
