using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;

/// <summary>
/// Billing-local reference to an order published by Ordering.
/// </summary>
public readonly record struct BilledOrderId(Guid Value)
{
    public static Either<DomainError, BilledOrderId> Create(Guid value) =>
        value == Guid.Empty
            ? Left<DomainError, BilledOrderId>(new BilledOrderIdRequiredError())
            : Right<DomainError, BilledOrderId>(new BilledOrderId(value));
}

public sealed record BilledOrderIdRequiredError()
    : DomainError(new DomainErrorCode("billing.order_required"), "Pedido faturável obrigatório.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
