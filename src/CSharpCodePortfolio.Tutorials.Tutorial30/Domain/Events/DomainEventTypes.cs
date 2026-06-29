namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Stable domain event names kept in constants to avoid scattered magic strings.
/// </summary>
public static class DomainEventTypes
{
    private const string UserRegisteredValue = "user.registered";
    private const string UserNameChangedValue = "user.name_changed";
    private const string UserEmailChangedValue = "user.email_changed";
    private const string UserPhoneNumberChangedValue = "user.phone_number_changed";

    /// <summary>
    /// Event type raised when a user registration aggregate is created.
    /// </summary>
    public static readonly DomainEventType UserRegistered = new(UserRegisteredValue);

    /// <summary>
    /// Event type raised when the user's name changes.
    /// </summary>
    public static readonly DomainEventType UserNameChanged = new(UserNameChangedValue);

    /// <summary>
    /// Event type raised when the user's required email changes.
    /// </summary>
    public static readonly DomainEventType UserEmailChanged = new(UserEmailChangedValue);

    /// <summary>
    /// Event type raised when the user's optional phone changes.
    /// </summary>
    public static readonly DomainEventType UserPhoneNumberChanged = new(UserPhoneNumberChangedValue);
}
