using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using System.Net.Mail;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;

/// <summary>
/// Value object for the required registration email.
/// </summary>
public readonly record struct Email(string Value)
{
    /// <summary>
    /// Validates an email that was actually supplied by the caller.
    /// </summary>
    public static Either<DomainError, Email> Create(Option<string> value)
    {
        return value.Match(
            Some: text => string.IsNullOrWhiteSpace(text)
                ? Left<DomainError, Email>(new EmailInvalidError())
                : TryNormalize(text),
            None: () => Left<DomainError, Email>(new EmailInvalidError()));
    }

    private static Either<DomainError, Email> TryNormalize(string value)
    {
        try
        {
            var normalized = value.Trim().ToLowerInvariant();
            var parsed = new MailAddress(normalized);

            return parsed.Address == normalized
                ? Right<DomainError, Email>(new Email(normalized))
                : Left<DomainError, Email>(new EmailInvalidError());
        }
        catch (FormatException)
        {
            return Left<DomainError, Email>(new EmailInvalidError());
        }
    }

    /// <summary>
    /// Returns the normalized email address for persistence and comparison.
    /// </summary>
    public override string ToString() => Value;
}

/// <summary>
/// Error returned when the required email is missing or malformed.
/// </summary>
public sealed record EmailInvalidError()
    : DomainError(new DomainErrorCode("registration.email_invalid"), "Email informado é inválido.")
{
    /// <inheritdoc />
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
