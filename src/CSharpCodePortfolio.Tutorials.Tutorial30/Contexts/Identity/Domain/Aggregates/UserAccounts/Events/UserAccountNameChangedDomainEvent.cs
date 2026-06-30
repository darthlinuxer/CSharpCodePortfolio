using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Events;

/// <summary>
/// Domain event raised when a registered user's name changes.
/// </summary>
public sealed record UserAccountNameChangedDomainEvent(
    Guid UserId,
    PersonName PreviousName,
    PersonName NewName,
    Timestamp OccurredAtUtc)
    : AbstractDomainEvent<UserAccount>("PersonName changed",OccurredAtUtc);
