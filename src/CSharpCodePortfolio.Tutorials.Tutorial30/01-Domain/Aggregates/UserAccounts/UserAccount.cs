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
public sealed class UserAccount : AbstractEntity<Guid, UserAccount>
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
        PhoneNumberValue = phoneNumber.ToNullable();
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
    public Option<PhoneNumber> PhoneNumber => PhoneNumberValue.ToOption();

    /// <summary>
    /// Gets the internal nullable phone state that EF Core can map while the public domain API stays Option-based.
    /// </summary>
    internal PhoneNumber? PhoneNumberValue { get; private set; }

    /// <summary>
    /// Creates a fully valid aggregate from raw command values and accumulates expected domain errors.
    /// </summary>
    public static Either<Seq<DomainError>, UserAccount> Create(
        string? name,
        string? document,
        string? email,
        string? phoneNumber)
    {
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
            select new UserAccount(userName, normalizedDocument, userEmail, userPhoneNumber, Timestamp.UtcNow);

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
    public Either<Seq<DomainError>, Unit> Rename(string? value)
    {
        return PersonName.Create(value).Match(
            Right: Rename,
            Left: error => Left<Seq<DomainError>, Unit>(OneError(error)));
    }

    /// <summary>
    /// Changes the user's required email and raises a typed domain event.
    /// </summary>
    public Either<Seq<DomainError>, Unit> ChangeEmail(string? value)
    {
        return Email.Create(value).Match(
            Right: ChangeEmail,
            Left: error => Left<Seq<DomainError>, Unit>(OneError(error)));
    }

    /// <summary>
    /// Changes the user's optional phone and raises a typed domain event.
    /// </summary>
    public Either<Seq<DomainError>, Unit> ChangePhoneNumber(string? value)
    {
        return PhoneNumberVo.CreateOptional(value).Match(
            Right: ChangePhoneNumber,
            Left: error => Left<Seq<DomainError>, Unit>(OneError(error)));
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
    private Either<Seq<DomainError>, Unit> Rename(PersonName newName)
    {
        if (newName == Name)
            return Right<Seq<DomainError>, Unit>(default);

        var previousName = Name;
        var occurredAtUtc = Timestamp.UtcNow;

        Name = newName;
        MarkModified(occurredAtUtc, None);
        RaiseDomainEvent(new UserAccountNameChangedDomainEvent(Id, previousName, newName, occurredAtUtc));

        return Right<Seq<DomainError>, Unit>(default);
    }

    /// <summary>
    /// Applies a valid email change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ChangeEmail(Email newEmail)
    {
        if (newEmail == Email)
            return Right<Seq<DomainError>, Unit>(default);

        var previousEmail = Email;
        var occurredAtUtc = Timestamp.UtcNow;

        Email = newEmail;
        MarkModified(occurredAtUtc, None);
        RaiseDomainEvent(new UserAccountEmailChangedDomainEvent(Id, previousEmail, newEmail, occurredAtUtc));

        return Right<Seq<DomainError>, Unit>(default);
    }

    /// <summary>
    /// Applies a valid optional phone change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ChangePhoneNumber(Option<PhoneNumber> newPhoneNumber)
    {
        var previousPhoneNumber = PhoneNumber;
        if (previousPhoneNumber == newPhoneNumber)
            return Right<Seq<DomainError>, Unit>(default);

        var occurredAtUtc = Timestamp.UtcNow;

        PhoneNumberValue = newPhoneNumber.ToNullable();
        MarkModified(occurredAtUtc, None);
        RaiseDomainEvent(new UserAccountPhoneNumberChangedDomainEvent(Id, previousPhoneNumber, newPhoneNumber, occurredAtUtc));

        return Right<Seq<DomainError>, Unit>(default);
    }
}
