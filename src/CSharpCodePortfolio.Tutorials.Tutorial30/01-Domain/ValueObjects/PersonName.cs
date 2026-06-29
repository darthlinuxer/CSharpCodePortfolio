
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
    /// Validates raw input and returns the same shape as aggregate-level
    /// errors (Seq, not single), so composition through <c>from x in ...</c>
    /// LINQ syntax is uniform across the domain.
    /// </summary>
    public static Either<Seq<DomainError>, PersonName> Create(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Left<Seq<DomainError>, PersonName>(Seq1(new PersonNameRequiredError() as DomainError))
            : Right<Seq<DomainError>, PersonName>(new PersonName(value.Trim()));
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