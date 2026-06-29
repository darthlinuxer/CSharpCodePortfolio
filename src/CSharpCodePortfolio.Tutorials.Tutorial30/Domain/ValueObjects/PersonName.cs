using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Value object that keeps the required user name non-null and normalized.
/// </summary>
public sealed record PersonName
{
    private PersonName()
    {
    }

    private PersonName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the normalized name stored by the domain.
    /// </summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Validates raw input and returns Either instead of throwing for expected user mistakes.
    /// </summary>
    public static Either<DomainError, PersonName> Create(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Left<DomainError, PersonName>(DomainErrors.NameRequired)
            : Right<DomainError, PersonName>(new PersonName(value.Trim()));
    }

    /// <summary>
    /// Returns the name value for console evidence.
    /// </summary>
    public override string ToString() => Value;
}
