using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using System.Net.Mail;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;

/// <summary>
/// Value object for the required registration email.
/// </summary>
public sealed record Email
{
    private Email()
    {
    }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the normalized email address.
    /// </summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Validates an email that was actually supplied by the caller.
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
    /// Rehydrates an already normalized value from a trusted storage boundary.
    /// </summary>
    internal static Email FromTrustedValue(string value) => new(value);

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
