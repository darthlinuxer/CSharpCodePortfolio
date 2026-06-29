using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Events;

/// <summary>
/// Domain event raised when the registration aggregate is created successfully.
/// </summary>
public sealed record UserAccountRegisteredDomainEvent(
    Guid UserId,
    string Document,
    Email Email,
    Timestamp OccurredAtUtc)
    : DomainEvent(UserAccountDomainEventTypes.Registered, OccurredAtUtc);
