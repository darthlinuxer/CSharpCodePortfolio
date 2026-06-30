using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
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

        var validName = PersonName.Create(name);
        var validEmail = Email.Create(email);
        var validPhoneNumber = PhoneNumberVo.CreateOptional(phoneNumber);
        var errors = FlattenErrors(Seq(
                ErrorOf(validName),
                ErrorOf(validEmail),
                ErrorOf(validPhoneNumber)));

        var account =
            from userName in validName
            from userEmail in validEmail
            from userPhoneNumber in validPhoneNumber
            select new UserAccount(userName, userEmail, userPhoneNumber, Timestamp.UtcNow(clock));

        return account.Match(
            Right: value => Right<Seq<DomainError>, UserAccount>(value),
            Left: error => Left<Seq<DomainError>, UserAccount>(errors.IsEmpty ? Seq1(error) : errors));
    }

    /// <summary>
    /// Decides registration uniqueness from persistence facts supplied by the application layer.
    /// </summary>
    public Either<DomainError, Unit> EnsureCanBeRegistered(bool emailExists)
    {
        return !emailExists
            ? Right<DomainError, Unit>(default)
            : Left<DomainError, Unit>(new UserAccountEmailDuplicateError());
    }

    /// <summary>
    /// Changes the user's required name and raises a typed domain event.
    /// </summary>
    public Either<DomainError, Unit> Rename(Option<string> value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return PersonName.Create(value).Bind(newName => Rename(newName, clock));
    }

    /// <summary>
    /// Changes the user's required email and raises a typed domain event.
    /// </summary>
    public Either<DomainError, Unit> ChangeEmail(Option<string> value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return Email.Create(value).Bind(newEmail => ChangeEmail(newEmail, clock));
    }

    /// <summary>
    /// Changes the user's optional phone and raises a typed domain event.
    /// </summary>
    public Either<DomainError, Unit> ChangePhoneNumber(Option<string> value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return PhoneNumberVo.CreateOptional(value).Bind(newPhoneNumber => ChangePhoneNumber(newPhoneNumber, clock));
    }

    /// <summary>
    /// Extracts an expected value-object error for aggregate-level accumulation.
    /// </summary>
    private static Option<DomainError> ErrorOf<T>(Either<DomainError, T> result)
    {
        return result.Match(
            Right: _ => None,
            Left: errors => Some(errors));
    }

    /// <summary>
    /// Flattens collected value-object validation errors.
    /// </summary>
    private static Seq<DomainError> FlattenErrors(Seq<Option<DomainError>> errors) =>
        errors.Bind(error => error.Match(Some: Seq1, None: Seq<DomainError>));

    /// <summary>
    /// Applies a valid name change after the factory has parsed the raw value.
    /// </summary>
    private Either<DomainError, Unit> Rename(PersonName newName, TimeProvider clock)
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
    /// Applies a valid email change after the factory has parsed the raw value.
    /// </summary>
    private Either<DomainError, Unit> ChangeEmail(Email newEmail, TimeProvider clock)
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
    /// Applies a valid optional phone change after the factory has parsed the raw value.
    /// </summary>
    private Either<DomainError, Unit> ChangePhoneNumber(Option<PhoneNumber> newPhoneNumber, TimeProvider clock)
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
