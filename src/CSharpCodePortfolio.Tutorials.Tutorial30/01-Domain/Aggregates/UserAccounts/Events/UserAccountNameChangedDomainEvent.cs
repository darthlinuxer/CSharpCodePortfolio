using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Events;

/// <summary>
/// Domain event raised when a registered user's name changes.
/// </summary>
public sealed record UserAccountNameChangedDomainEvent(
    Guid UserId,
    PersonName PreviousName,
    PersonName NewName,
    Timestamp OccurredAtUtc)
    : DomainEvent(UserAccountDomainEventTypes.NameChanged, OccurredAtUtc);
