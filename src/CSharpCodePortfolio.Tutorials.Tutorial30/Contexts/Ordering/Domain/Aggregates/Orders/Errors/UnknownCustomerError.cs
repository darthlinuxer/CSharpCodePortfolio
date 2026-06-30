using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;

public sealed record UnknownCustomerError()
    : DomainError(new DomainErrorCode("ordering.customer_unknown"), "Cliente não existe no contexto Ordering.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.NotFound;
}
