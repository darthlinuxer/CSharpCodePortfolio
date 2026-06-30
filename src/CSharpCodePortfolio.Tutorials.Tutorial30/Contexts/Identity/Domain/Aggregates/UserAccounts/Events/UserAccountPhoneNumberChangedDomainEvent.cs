using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Events;

/// <summary>
/// Domain event raised when a registered user's optional phone changes.
/// </summary>
public sealed record UserAccountPhoneNumberChangedDomainEvent(
    Guid UserId,
    Option<PhoneNumber> PreviousPhoneNumber,
    Option<PhoneNumber> NewPhoneNumber,
    Timestamp OccurredAtUtc)
    : AbstractDomainEvent<UserAccount>("Phone changed", OccurredAtUtc);
