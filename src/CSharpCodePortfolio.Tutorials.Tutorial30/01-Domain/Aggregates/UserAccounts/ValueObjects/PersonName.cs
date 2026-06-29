using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;

/// <summary>
/// Value object that keeps the required user name non-null and normalized.
/// </summary>
public readonly record struct PersonName(string Value)
{
    /// <summary>
    /// Validates raw input and returns Either instead of throwing for expected user mistakes.
    /// </summary>
    public static Either<Seq<DomainError>, PersonName> Create(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Left<Seq<DomainError>, PersonName>(Seq1<DomainError>(new PersonNameRequiredError()))
            : Right<Seq<DomainError>, PersonName>(new PersonName(value.Trim()));
    }

    /// <summary>
    /// Returns the name value for console evidence.
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
