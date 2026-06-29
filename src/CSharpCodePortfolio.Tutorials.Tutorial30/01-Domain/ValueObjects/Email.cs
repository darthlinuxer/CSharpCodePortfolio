using System.Net.Mail;

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
    /// Validates and normalizes an email that was actually supplied by the
    /// caller. Returns the same shape as aggregate-level errors
    /// (Seq, not single).
    /// </summary>
    public static Either<Seq<DomainError>, Email> Create(string? value)
    {
        return value is not { Length: > 0 } || string.IsNullOrWhiteSpace(value)
            ? Left<Seq<DomainError>, Email>(Seq1(new EmailInvalidError() as DomainError))
            : TryNormalize(value);

        static Either<Seq<DomainError>, Email> TryNormalize(string source)
        {
            try
            {
                var normalized = source.Trim().ToLowerInvariant();
                var parsed = new MailAddress(normalized);

                return parsed.Address == normalized
                    ? Right<Seq<DomainError>, Email>(new Email(normalized))
                    : Left<Seq<DomainError>, Email>(Seq1(new EmailInvalidError() as DomainError));
            }
            catch (FormatException)
            {
                return Left<Seq<DomainError>, Email>(Seq1(new EmailInvalidError() as DomainError));
            }
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