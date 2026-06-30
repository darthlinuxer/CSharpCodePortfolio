using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;

/// <summary>
/// Billing-local reference to the customer in a published order contract.
/// </summary>
public readonly record struct BillingCustomerId(Guid Value)
{
    public static Either<DomainError, BillingCustomerId> Create(Guid value) =>
        value == Guid.Empty
            ? Left<DomainError, BillingCustomerId>(new BillingCustomerIdRequiredError())
            : Right<DomainError, BillingCustomerId>(new BillingCustomerId(value));
}

public sealed record BillingCustomerIdRequiredError()
    : DomainError(new DomainErrorCode("billing.customer_required"), "Cliente faturável obrigatório.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
