using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Events;

/// <summary>
/// Stable user account domain event names kept in constants to avoid scattered magic strings.
/// </summary>
public static class UserAccountDomainEventTypes
{
    private const string RegisteredValue = "user_account.registered";
    private const string NameChangedValue = "user_account.name_changed";
    private const string EmailChangedValue = "user_account.email_changed";
    private const string PhoneNumberChangedValue = "user_account.phone_number_changed";

    /// <summary>
    /// Event type raised when a user registration aggregate is created.
    /// </summary>
    public static readonly DomainEventType Registered = new(RegisteredValue);

    /// <summary>
    /// Event type raised when the user's name changes.
    /// </summary>
    public static readonly DomainEventType NameChanged = new(NameChangedValue);

    /// <summary>
    /// Event type raised when the user's required email changes.
    /// </summary>
    public static readonly DomainEventType EmailChanged = new(EmailChangedValue);

    /// <summary>
    /// Event type raised when the user's optional phone changes.
    /// </summary>
    public static readonly DomainEventType PhoneNumberChanged = new(PhoneNumberChangedValue);
}
