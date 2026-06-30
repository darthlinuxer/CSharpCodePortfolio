using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;

public sealed record OrderCannotBeChangedError(OrderStatus Status)
    : DomainError(
        new DomainErrorCode("ordering.order_cannot_be_changed"),
        $"Pedido em status {Status} não pode ser alterado.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}
