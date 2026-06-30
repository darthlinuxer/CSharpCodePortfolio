using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Events;

/// <summary>
/// Domain event raised when a registered user's optional phone changes.
/// </summary>
public sealed record UserAccountPhoneNumberChangedDomainEvent(
    Guid UserId,
    Option<PhoneNumber> PreviousPhoneNumber,
    Option<PhoneNumber> NewPhoneNumber,
    Timestamp OccurredAtUtc)
    : AbstractDomainEvent<UserAccount>("Phone changed", OccurredAtUtc);
