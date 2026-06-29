using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;

/// <summary>
/// Value object for optional phone numbers represented as Option&lt;PhoneNumber&gt; in the aggregate.
/// </summary>
public readonly record struct PhoneNumber(string Value)
{
    /// <summary>
    /// Validates an optional phone, making absence explicit with None.
    /// </summary>
    public static Either<Seq<DomainError>, Option<PhoneNumber>> CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Right<Seq<DomainError>, Option<PhoneNumber>>(None)
            : Normalize(value);
    }

    private static Either<Seq<DomainError>, Option<PhoneNumber>> Normalize(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());

        return digits is { Length: >= 10 and <= 15 }
            ? Right<Seq<DomainError>, Option<PhoneNumber>>(Some(new PhoneNumber(digits)))
            : Left<Seq<DomainError>, Option<PhoneNumber>>(Seq1<DomainError>(new PhoneNumberInvalidError()));
    }

    /// <summary>
    /// Returns the normalized phone for DTO and persistence mapping.
    /// </summary>
    public override string ToString() => Value;
}

/// <summary>
/// Error returned when an optional phone was supplied but is malformed.
/// </summary>
public sealed record PhoneNumberInvalidError()
    : DomainError(new DomainErrorCode("registration.phone_invalid"), "Telefone informado é inválido.")
{
    /// <inheritdoc />
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
