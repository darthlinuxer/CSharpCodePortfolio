using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;

public sealed record OrderAlreadyConfirmedError()
    : DomainError(new DomainErrorCode("ordering.order_already_confirmed"), "Pedido já foi confirmado.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}
