using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Functional;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;
using LanguageExt;
using static LanguageExt.Prelude;
using PhoneNumberVo = CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects.PhoneNumber;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts;

/// <summary>
/// Aggregate root for registration; required values are direct VOs and optional values are explicit Option VOs.
/// </summary>
public sealed class UserAccount : AbstractEntity<Guid>
{
    private UserAccount()
    {
    }

    private UserAccount(
        PersonName name,
        Email email,
        Option<PhoneNumber> phoneNumber,
        Timestamp registeredAtUtc)
    {
        Name = name;
        Email = email;
        PhoneNumberValue = phoneNumber.ToNullable();
        MarkCreated(registeredAtUtc);
        RaiseDomainEvent(new UserAccountRegisteredDomainEvent(Id, email, registeredAtUtc));
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
    /// Gets the optional phone without using PhoneNumber?.
    /// </summary>
    public Option<PhoneNumber> PhoneNumber => PhoneNumberValue.HasValue ? Some(PhoneNumberValue.Value) : None;

    /// <summary>
    /// Gets the nullable shape EF Core maps while the public domain API stays Option-based.
    /// </summary>
    internal PhoneNumber? PhoneNumberValue { get; private set; }

    /// <summary>
    /// Creates a fully valid aggregate from raw command values and accumulates expected domain errors.
    /// </summary>
    public static Either<Seq<DomainError>, UserAccount> Create(
        string? name,
        string? email,
        string? phoneNumber,
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

        return errors.IsEmpty
            ? account
            : Left<Seq<DomainError>, UserAccount>(errors);
    }

    /// <summary>
    /// Decides registration uniqueness from persistence facts supplied by the application layer.
    /// </summary>
    public Either<Seq<DomainError>, Unit> EnsureCanBeRegistered(bool emailExists)
    {
        return !emailExists
            ? Right<Seq<DomainError>, Unit>(default)
            : Left<Seq<DomainError>, Unit>(Seq1<DomainError>(new UserAccountEmailDuplicateError()));
    }

    /// <summary>
    /// Changes the user's required name and raises a typed domain event.
    /// </summary>
    public Either<Seq<DomainError>, Unit> Rename(string? value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return PersonName.Create(value).Bind(newName => Rename(newName, clock));
    }

    /// <summary>
    /// Changes the user's required email and raises a typed domain event.
    /// </summary>
    public Either<Seq<DomainError>, Unit> ChangeEmail(string? value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return Email.Create(value).Bind(newEmail => ChangeEmail(newEmail, clock));
    }

    /// <summary>
    /// Changes the user's optional phone and raises a typed domain event.
    /// </summary>
    public Either<Seq<DomainError>, Unit> ChangePhoneNumber(string? value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return PhoneNumberVo.CreateOptional(value).Bind(newPhoneNumber => ChangePhoneNumber(newPhoneNumber, clock));
    }

    /// <summary>
    /// Extracts an expected value-object error for aggregate-level accumulation.
    /// </summary>
    private static Option<Seq<DomainError>> ErrorOf<T>(Either<Seq<DomainError>, T> result)
    {
        return result.Match(
            Right: _ => None,
            Left: errors => Some(errors));
    }

    /// <summary>
    /// Flattens collected value-object validation errors.
    /// </summary>
    private static Seq<DomainError> FlattenErrors(Seq<Option<Seq<DomainError>>> errors) =>
        errors.Bind(error => error.Match(Some: item => item, None: Seq<DomainError>));

    /// <summary>
    /// Applies a valid name change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> Rename(PersonName newName, TimeProvider clock)
    {
        return ApplyIfChanged(
            current: Name,
            next: newName,
            clock,
            apply: occurredAtUtc =>
            {
                var previousName = Name;
                Name = newName;
                MarkModified(occurredAtUtc);
                RaiseDomainEvent(new UserAccountNameChangedDomainEvent(Id, previousName, newName, occurredAtUtc));
            });
    }

    /// <summary>
    /// Applies a valid email change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ChangeEmail(Email newEmail, TimeProvider clock)
    {
        return ApplyIfChanged(
            current: Email,
            next: newEmail,
            clock,
            apply: occurredAtUtc =>
            {
                var previousEmail = Email;
                Email = newEmail;
                MarkModified(occurredAtUtc);
                RaiseDomainEvent(new UserAccountEmailChangedDomainEvent(Id, previousEmail, newEmail, occurredAtUtc));
            });
    }

    /// <summary>
    /// Applies a valid optional phone change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ChangePhoneNumber(Option<PhoneNumber> newPhoneNumber, TimeProvider clock)
    {
        return ApplyIfChanged(
            current: PhoneNumber,
            next: newPhoneNumber,
            clock,
            apply: occurredAtUtc =>
            {
                var previousPhoneNumber = PhoneNumber;
                PhoneNumberValue = newPhoneNumber.ToNullable();
                MarkModified(occurredAtUtc);
                RaiseDomainEvent(new UserAccountPhoneNumberChangedDomainEvent(Id, previousPhoneNumber, newPhoneNumber, occurredAtUtc));
            });
    }

    private static Either<Seq<DomainError>, Unit> ApplyIfChanged<T>(
        T current,
        T next,
        TimeProvider clock,
        Action<Timestamp> apply)
    {
        return EqualityComparer<T>.Default.Equals(current, next)
            ? Right<Seq<DomainError>, Unit>(default)
            : ApplyChange(clock, apply);
    }

    private static Either<Seq<DomainError>, Unit> ApplyChange(TimeProvider clock, Action<Timestamp> apply)
    {
        apply(Timestamp.UtcNow(clock));
        return Right<Seq<DomainError>, Unit>(default);
    }
}
