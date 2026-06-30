using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;

/// <summary>
/// Strongly typed order identity.
/// </summary>
public readonly record struct OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.CreateVersion7());

    public static Either<DomainError, OrderId> Create(Guid value) =>
        value == Guid.Empty
            ? Left<DomainError, OrderId>(new OrderIdRequiredError())
            : Right<DomainError, OrderId>(new OrderId(value));

    public override string ToString() => Value.ToString();
}

public sealed record OrderIdRequiredError()
    : DomainError(new DomainErrorCode("ordering.order_required"), "Pedido obrigatório.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
