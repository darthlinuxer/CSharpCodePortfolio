using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Optional value object for phone numbers. Modeled as a
/// <c>readonly record struct</c> so absence in the aggregate is expressed
/// by <see cref="Option{PhoneNumber}"/> (a domain-level concept) instead of
/// <see cref="Nullable{T}"/> (a runtime-level concept).
/// </summary>
public readonly record struct PhoneNumber(string Value)
{
    /// <summary>
    /// Validates an optional phone, making absence explicit with
    /// <c>Option&lt;PhoneNumber&gt;.None</c>.
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
    /// Returns the normalized phone digits for DTO mapping.
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