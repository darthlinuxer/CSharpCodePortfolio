using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

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
            : Left<DomainError, Option<PhoneNumber>>(DomainErrors.PhoneNumberInvalid);
    }

    /// <summary>
    /// Returns the normalized phone for DTO and persistence mapping.
    /// </summary>
    public override string ToString() => Value;
}
