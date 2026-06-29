using System.Net.Mail;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Required value object for the registration email. Modeled as a
/// <c>readonly record struct</c> so that normalized equality is structural,
/// allocation-free, and the contract (always present and normalized) is
/// enforced by the smart constructor.
/// </summary>
public readonly record struct Email(string Value)
{
    /// <summary>
    /// Validates and normalizes an email that was actually supplied by
    /// the caller. Returns <see cref="Either{DomainError, Email}"/> so the
    /// caller must close the error path explicitly.
    /// </summary>
    public static Either<DomainError, Email> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Left<DomainError, Email>(new EmailInvalidError());
        }

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
    : DomainError(new DomainErrorCode("registration.email_invalid"), "Email informado é inválido.");