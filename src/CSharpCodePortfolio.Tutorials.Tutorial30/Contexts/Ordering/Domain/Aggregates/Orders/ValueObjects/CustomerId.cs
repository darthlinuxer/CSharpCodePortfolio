using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;

/// <summary>
/// Ordering-local reference to a registered user published by Identity.
/// </summary>
public readonly record struct CustomerId(Guid Value)
{
    public static Either<DomainError, CustomerId> Create(Guid value) =>
        value == Guid.Empty
            ? Left<DomainError, CustomerId>(new CustomerIdRequiredError())
            : Right<DomainError, CustomerId>(new CustomerId(value));

    public override string ToString() => Value.ToString();
}

public sealed record CustomerIdRequiredError()
    : DomainError(new DomainErrorCode("ordering.customer_required"), "Cliente obrigatório.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
