using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;

/// <summary>
/// Positive monetary amount used by Ordering and Billing.
/// </summary>
public readonly record struct Money(decimal Value)
{
    public static Either<DomainError, Money> Create(decimal value) =>
        value <= 0
            ? Left<DomainError, Money>(new MoneyPositiveRequiredError())
            : Right<DomainError, Money>(new Money(decimal.Round(value, 2, MidpointRounding.AwayFromZero)));

    public static Money Sum(IEnumerable<Money> values) => new(values.Sum(value => value.Value));

    public override string ToString() => Value.ToString("0.00");
}

public sealed record MoneyPositiveRequiredError()
    : DomainError(new DomainErrorCode("ordering.money_positive_required"), "Valor deve ser positivo.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
