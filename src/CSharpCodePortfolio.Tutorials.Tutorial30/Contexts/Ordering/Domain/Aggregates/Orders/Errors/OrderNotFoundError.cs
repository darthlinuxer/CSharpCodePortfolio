using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;

public sealed record OrderNotFoundError()
    : DomainError(new DomainErrorCode("ordering.order_not_found"), "Pedido não encontrado.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.NotFound;
}
