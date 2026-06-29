using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using System.Net.Mail;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;

/// <summary>
/// Value object for the required registration email.
/// </summary>
public readonly record struct Email(string Value)
{
    /// <summary>
    /// Validates an email that was actually supplied by the caller.
    /// </summary>
    public static Either<Seq<DomainError>, Email> Create(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Left<Seq<DomainError>, Email>(Seq1<DomainError>(new EmailInvalidError()))
            : TryNormalize(value);
    }

    private static Either<Seq<DomainError>, Email> TryNormalize(string value)
    {
        try
        {
            var normalized = value.Trim().ToLowerInvariant();
            var parsed = new MailAddress(normalized);

            return parsed.Address == normalized
                ? Right<Seq<DomainError>, Email>(new Email(normalized))
                : Left<Seq<DomainError>, Email>(Seq1<DomainError>(new EmailInvalidError()));
        }
        catch (FormatException)
        {
            return Left<Seq<DomainError>, Email>(Seq1<DomainError>(new EmailInvalidError()));
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
