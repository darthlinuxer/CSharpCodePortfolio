using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;

public sealed record OrderLineNotFoundError()
    : DomainError(new DomainErrorCode("ordering.order_line_not_found"), "Item do pedido não encontrado.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.NotFound;
}
