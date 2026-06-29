using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;

/// <summary>
/// Value object for optional phone numbers represented as Option&lt;PhoneNumber&gt; in the aggregate.
/// </summary>
public sealed record PhoneNumber
{
    private PhoneNumber()
    {
    }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the normalized phone digits.
    /// </summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Validates an optional phone, making absence explicit with None.
    /// </summary>
    public static Either<DomainError, Option<PhoneNumber>> CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Right<DomainError, Option<PhoneNumber>>(None);
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());

        return digits is { Length: >= 10 and <= 15 }
            ? Right<DomainError, Option<PhoneNumber>>(Some(new PhoneNumber(digits)))
            : Left<DomainError, Option<PhoneNumber>>(new PhoneNumberInvalidError());
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
    : DomainError(new DomainErrorCode("registration.phone_invalid"), "Telefone informado é inválido.");
