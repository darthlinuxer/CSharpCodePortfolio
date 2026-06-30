using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;

/// <summary>
/// Positive item quantity.
/// </summary>
public readonly record struct Quantity(int Value)
{
    public static Either<DomainError, Quantity> Create(int value) =>
        value <= 0
            ? Left<DomainError, Quantity>(new QuantityPositiveRequiredError())
            : Right<DomainError, Quantity>(new Quantity(value));

    public override string ToString() => Value.ToString();
}

public sealed record QuantityPositiveRequiredError()
    : DomainError(new DomainErrorCode("ordering.quantity_positive_required"), "Quantidade deve ser positiva.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
