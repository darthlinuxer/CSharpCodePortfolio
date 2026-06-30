using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Functional;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using static LanguageExt.Prelude;
using PhoneNumberVo = CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects.PhoneNumber;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;

/// <summary>
/// Aggregate root for registration; required values are direct VOs and optional values are explicit Option VOs.
/// </summary>
public sealed class UserAccount : AbstractAggregate<UserAccount, Guid>
{
    private UserAccount()
    {
    }

    private UserAccount(
        PersonName name,
        Email email,
        Option<PhoneNumber> phoneNumber,
        Timestamp registeredAtUtc)
        : base(Guid.CreateVersion7())
    {
        Name = name;
        Email = email;
        PhoneNumberValue = phoneNumber;
        RecordCreated(registeredAtUtc, occurredAtUtc => new UserAccountRegisteredDomainEvent(Id, email, occurredAtUtc));
    }

    /// <summary>
    /// Gets the required non-null name.
    /// </summary>
    public PersonName Name { get; private set; }

    /// <summary>
    /// Gets the required non-null email.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Gets the optional phone without using nullable state.
    /// </summary>
    public Option<PhoneNumber> PhoneNumber => PhoneNumberValue;

    /// <summary>
    /// Gets the EF-mapped optional phone while the domain model stays monadic.
    /// </summary>
    internal Option<PhoneNumber> PhoneNumberValue { get; private set; } = None;

    /// <summary>
    /// Creates a fully valid aggregate from raw command values and accumulates expected domain errors.
    /// </summary>
    public static Either<Seq<DomainError>, UserAccount> Create(
        Option<string> name,
        Option<string> email,
        Option<string> phoneNumber,
        TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return (
            PersonName.Create(name),
            Email.Create(email),
            PhoneNumberVo.CreateOptional(phoneNumber))
            .Combine((userName, userEmail, userPhoneNumber) =>
                new UserAccount(userName, userEmail, userPhoneNumber, Timestamp.UtcNow(clock)));
    }

    public static Either<DomainError, Unit> EnsureEmailIsAvailable(bool emailAlreadyExists) =>
        (!emailAlreadyExists).Ensure(() => new UserAccountEmailDuplicateError());

    public Either<DomainError, Unit> EnsureEmailCanChangeTo(Email newEmail, bool emailAlreadyExists) =>
        EqualityComparer<Email>.Default.Equals(Email, newEmail)
            ? Right<DomainError, Unit>(default)
            : EnsureEmailIsAvailable(emailAlreadyExists);

    /// <summary>
    /// Changes the user's required name and raises a typed domain event.
    /// </summary>
    public Either<DomainError, Unit> Rename(PersonName newName, TimeProvider clock)
    {
        return ApplyChangeIfDifferent(
            current: Name,
            next: newName,
            clock,
            apply: value => Name = value,
            createEvent: (previousName, changedName, occurredAtUtc) =>
                new UserAccountNameChangedDomainEvent(Id, previousName, changedName, occurredAtUtc));
    }

    /// <summary>
    /// Changes the user's required email and raises a typed domain event.
    /// </summary>
    public Either<DomainError, Unit> ChangeEmail(Email newEmail, TimeProvider clock)
    {
        return ApplyChangeIfDifferent(
            current: Email,
            next: newEmail,
            clock,
            apply: value => Email = value,
            createEvent: (previousEmail, changedEmail, occurredAtUtc) =>
                new UserAccountEmailChangedDomainEvent(Id, previousEmail, changedEmail, occurredAtUtc));
    }

    /// <summary>
    /// Changes the user's optional phone and raises a typed domain event.
    /// </summary>
    public Either<DomainError, Unit> ChangePhoneNumber(Option<PhoneNumber> newPhoneNumber, TimeProvider clock)
    {
        return ApplyChangeIfDifferent(
            current: PhoneNumber,
            next: newPhoneNumber,
            clock,
            apply: value => PhoneNumberValue = value,
            createEvent: (previousPhoneNumber, changedPhoneNumber, occurredAtUtc) =>
                new UserAccountPhoneNumberChangedDomainEvent(Id, previousPhoneNumber, changedPhoneNumber, occurredAtUtc));
    }
}
