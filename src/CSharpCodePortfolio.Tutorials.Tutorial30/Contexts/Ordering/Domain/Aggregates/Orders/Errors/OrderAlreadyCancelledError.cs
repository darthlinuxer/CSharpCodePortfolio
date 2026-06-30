using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;

public sealed record OrderAlreadyCancelledError()
    : DomainError(new DomainErrorCode("ordering.order_already_cancelled"), "Pedido já foi cancelado.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}
