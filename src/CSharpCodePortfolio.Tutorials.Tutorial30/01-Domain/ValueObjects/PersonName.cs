using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Required value object that keeps the natural-person display name non-null
/// and trimmed. Modeled as a <c>readonly record struct</c> so that
/// equality is structural by value, no allocation is required per instance,
/// and the absence-vs-presence contract (always present) stays at the type
/// level rather than at the nullability level.
/// </summary>
public readonly record struct PersonName(string Value)
{
    /// <summary>
    /// Validates raw input and returns Either instead of throwing for
    /// expected user mistakes.
    /// </summary>
    public static Either<DomainError, PersonName> Create(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Left<DomainError, PersonName>(new PersonNameRequiredError())
            : Right<DomainError, PersonName>(new PersonName(value.Trim()));
    }

    /// <summary>
    /// Returns the name value for console evidence and DTO mapping.
    /// </summary>
    public override string ToString() => Value;
}

/// <summary>
/// Error returned when the required user name is missing.
/// </summary>
public sealed record PersonNameRequiredError()
    : DomainError(new DomainErrorCode("registration.name_required"), "Nome obrigatório.")
{
    /// <inheritdoc />
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}