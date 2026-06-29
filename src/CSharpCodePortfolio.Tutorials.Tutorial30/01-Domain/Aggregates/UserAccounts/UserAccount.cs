using LanguageExt;
using PhoneNumberVo = CSharpCodePortfolio.Tutorials.Tutorial30.Domain.PhoneNumber;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

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
        string document,
        Email email,
        Option<PhoneNumber> phoneNumber,
        Timestamp registeredAtUtc)
    {
        Name = name;
        Document = document;
        Email = email;
        PhoneNumberValue = ToNullable(phoneNumber);
        MarkCreated(registeredAtUtc, None);
        RaiseDomainEvent(new UserAccountRegisteredDomainEvent(Id, document, email, registeredAtUtc));
    }

    /// <summary>
    /// Gets the required non-null name.
    /// </summary>
    public PersonName Name { get; private set; } = null!;

    /// <summary>
    /// Gets the required normalized document string without an extra persistence-only value object.
    /// </summary>
    public string Document { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the required non-null email.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Gets the optional phone without using PhoneNumber?.
    /// </summary>
    public Option<PhoneNumber> PhoneNumber => ToOption(PhoneNumberValue);

    /// <summary>
    /// Gets the internal nullable phone state that EF Core can map while the public domain API stays Option-based.
    /// </summary>
    internal PhoneNumber? PhoneNumberValue { get; private set; }

    /// <summary>
    /// Creates a fully valid aggregate from raw command values and accumulates expected domain errors.
    /// </summary>
    /// <remarks>
    /// <paramref name="clock"/> defaults to <see cref="TimeProvider.System"/> for backward
    /// compatibility with the legacy call surface; the application layer will pass a real
    /// injected clock in Task 11.
    /// </remarks>
    public static Either<Seq<DomainError>, UserAccount> Create(
        string? name,
        string? document,
        string? email,
        string? phoneNumber,
        TimeProvider? clock = null)
    {
        var effectiveClock = clock ?? TimeProvider.System;
        var validName = PersonName.Create(name);
        var validDocument = NormalizeDocument(document);
        var validEmail = Email.Create(email);
        var validPhoneNumber = PhoneNumberVo.CreateOptional(phoneNumber);
        var errors = Seq(
                ErrorOf(validName),
                ErrorOf(validDocument),
                ErrorOf(validEmail),
                ErrorOf(validPhoneNumber))
            .Somes()
            .ToSeq();

        if (!errors.IsEmpty)
            return Left<Seq<DomainError>, UserAccount>(errors);

        var account =
            from userName in validName
            from normalizedDocument in validDocument
            from userEmail in validEmail
            from userPhoneNumber in validPhoneNumber
            select new UserAccount(userName, normalizedDocument, userEmail, userPhoneNumber, Timestamp.UtcNow(effectiveClock));

        return account.MapLeft(OneError);
    }

    /// <summary>
    /// Normalizes the required document without keeping a dedicated DocumentNumber type.
    /// </summary>
    public static Either<DomainError, string> NormalizeDocument(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());

        return digits is { Length: >= 5 and <= 20 }
            ? Right<DomainError, string>(digits)
            : Left<DomainError, string>(new UserAccountDocumentInvalidError());
    }

    /// <summary>
    /// Decides registration uniqueness from persistence facts supplied by the application layer.
    /// </summary>
    public Either<Seq<DomainError>, Unit> EnsureCanBeRegistered(bool documentExists, bool emailExists)
    {
        var errors = Seq(
                documentExists ? Some<DomainError>(new UserAccountDocumentDuplicateError()) : None,
                emailExists ? Some<DomainError>(new UserAccountEmailDuplicateError()) : None)
            .Somes()
            .ToSeq();

        return errors.IsEmpty
            ? Right<Seq<DomainError>, Unit>(default)
            : Left<Seq<DomainError>, Unit>(errors);
    }

    /// <summary>
    /// Changes the user's required name and raises a typed domain event.
    /// </summary>
    /// <remarks>
    /// <paramref name="clock"/> defaults to <see cref="TimeProvider.System"/> for backward
    /// compatibility with the legacy call surface; the application layer will pass a real
    /// injected clock in Task 11.
    /// </remarks>
    public Either<Seq<DomainError>, Unit> Rename(string? value, TimeProvider? clock = null)
    {
        var effectiveClock = clock ?? TimeProvider.System;
        return PersonName.Create(value).Match(
            Right: newName => Rename(newName, effectiveClock),
            Left: error => Left<Seq<DomainError>, Unit>(OneError(error)));
    }

    /// <summary>
    /// Changes the user's required email and raises a typed domain event.
    /// </summary>
    /// <remarks>
    /// <paramref name="clock"/> defaults to <see cref="TimeProvider.System"/> for backward
    /// compatibility with the legacy call surface; the application layer will pass a real
    /// injected clock in Task 11.
    /// </remarks>
    public Either<Seq<DomainError>, Unit> ChangeEmail(string? value, TimeProvider? clock = null)
    {
        var effectiveClock = clock ?? TimeProvider.System;
        return Email.Create(value).Match(
            Right: newEmail => ChangeEmail(newEmail, effectiveClock),
            Left: error => Left<Seq<DomainError>, Unit>(OneError(error)));
    }

    /// <summary>
    /// Changes the user's optional phone and raises a typed domain event.
    /// </summary>
    /// <remarks>
    /// <paramref name="clock"/> defaults to <see cref="TimeProvider.System"/> for backward
    /// compatibility with the legacy call surface; the application layer will pass a real
    /// injected clock in Task 11.
    /// </remarks>
    public Either<Seq<DomainError>, Unit> ChangePhoneNumber(string? value, TimeProvider? clock = null)
    {
        var effectiveClock = clock ?? TimeProvider.System;
        return PhoneNumberVo.CreateOptional(value).Match(
            Right: newPhone => ChangePhoneNumber(newPhone, effectiveClock),
            Left: error => Left<Seq<DomainError>, Unit>(OneError(error)));
    }

    /// <summary>
    /// Converts a nullable EF materialized value into an explicit domain Option.
    /// </summary>
    private static Option<T> ToOption<T>(T? value)
        where T : class
    {
        return value is null ? None : Some(value);
    }

    /// <summary>
    /// Converts an Option value into the nullable complex type EF Core maps.
    /// </summary>
    private static T? ToNullable<T>(Option<T> option)
        where T : class
    {
        foreach (var value in option)
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Extracts an expected value-object error for aggregate-level accumulation.
    /// </summary>
    private static Option<DomainError> ErrorOf<T>(Either<DomainError, T> result)
    {
        return result.Match(
            Right: _ => None,
            Left: error => Some(error));
    }

    /// <summary>
    /// Wraps a single error so aggregate methods expose one consistent Result shape.
    /// </summary>
    private static Seq<DomainError> OneError(DomainError error) => Seq1(error);

    /// <summary>
    /// Applies a valid name change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> Rename(PersonName newName, TimeProvider clock)
    {
        if (newName == Name)
            return Right<Seq<DomainError>, Unit>(default);

        var previousName = Name;
        var occurredAtUtc = Timestamp.UtcNow(clock);

        Name = newName;
        MarkModified(occurredAtUtc, None);
        RaiseDomainEvent(new UserAccountNameChangedDomainEvent(Id, previousName, newName, occurredAtUtc));

        return Right<Seq<DomainError>, Unit>(default);
    }

    /// <summary>
    /// Applies a valid email change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ChangeEmail(Email newEmail, TimeProvider clock)
    {
        if (newEmail == Email)
            return Right<Seq<DomainError>, Unit>(default);

        var previousEmail = Email;
        var occurredAtUtc = Timestamp.UtcNow(clock);

        Email = newEmail;
        MarkModified(occurredAtUtc, None);
        RaiseDomainEvent(new UserAccountEmailChangedDomainEvent(Id, previousEmail, newEmail, occurredAtUtc));

        return Right<Seq<DomainError>, Unit>(default);
    }

    /// <summary>
    /// Applies a valid optional phone change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ChangePhoneNumber(Option<PhoneNumber> newPhoneNumber, TimeProvider clock)
    {
        var previousPhoneNumber = PhoneNumber;
        if (previousPhoneNumber == newPhoneNumber)
            return Right<Seq<DomainError>, Unit>(default);

        var occurredAtUtc = Timestamp.UtcNow(clock);

        PhoneNumberValue = ToNullable(newPhoneNumber);
        MarkModified(occurredAtUtc, None);
        RaiseDomainEvent(new UserAccountPhoneNumberChangedDomainEvent(Id, previousPhoneNumber, newPhoneNumber, occurredAtUtc));

        return Right<Seq<DomainError>, Unit>(default);
    }
}
