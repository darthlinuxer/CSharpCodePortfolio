using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.Events;

public sealed record InvoiceCreatedDomainEvent(
    InvoiceId InvoiceId,
    BilledOrderId OrderId,
    BillingCustomerId CustomerId,
    InvoiceAmount Amount,
    Timestamp OccurredAtUtc)
    : AbstractDomainEvent<Invoice>("Invoice created", OccurredAtUtc);
