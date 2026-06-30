using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Events;

/// <summary>
/// Domain event raised when a registered user's required email changes.
/// </summary>
public sealed record UserAccountEmailChangedDomainEvent(
    Guid UserId,
    Email PreviousEmail,
    Email NewEmail,
    Timestamp OccurredAtUtc)
    : AbstractDomainEvent<UserAccount>("Email changed",OccurredAtUtc);
