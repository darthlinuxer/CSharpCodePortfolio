using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;

public sealed record OrderLineRequiredError()
    : DomainError(new DomainErrorCode("ordering.order_line_required"), "Pedido precisa de ao menos um item.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
