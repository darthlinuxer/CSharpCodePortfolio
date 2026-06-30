using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Events;

/// <summary>
/// Domain event raised when the registration aggregate is created successfully.
/// </summary>
public sealed record UserAccountRegisteredDomainEvent(
    Guid UserId,
    Email Email,
    Timestamp OccurredAtUtc)
    : AbstractDomainEvent<UserAccount>("User account created", OccurredAtUtc);
