using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;

/// <summary>
/// Billing aggregate created from Ordering's published language.
/// </summary>
public sealed class Invoice : AbstractAggregate<Invoice, InvoiceId>
{
    private Invoice()
        : base(InvoiceId.New())
    {
    }

    private Invoice(
        InvoiceId id,
        BilledOrderId orderId,
        BillingCustomerId customerId,
        InvoiceAmount amount,
        Guid sourceIntegrationEventId,
        Timestamp createdAtUtc)
        : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        SourceIntegrationEventId = sourceIntegrationEventId;
        Status = InvoiceStatus.Open;
        RecordCreated(createdAtUtc, occurredAtUtc => new InvoiceCreatedDomainEvent(Id, OrderId, CustomerId, Amount, occurredAtUtc));
    }

    public BilledOrderId OrderId { get; private set; }

    public BillingCustomerId CustomerId { get; private set; }

    public InvoiceAmount Amount { get; private set; }

    public Guid SourceIntegrationEventId { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public static Either<Seq<DomainError>, Invoice> Create(
        BilledOrderId orderId,
        BillingCustomerId customerId,
        InvoiceAmount amount,
        Guid sourceIntegrationEventId,
        TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return sourceIntegrationEventId == Guid.Empty
            ? Left<Seq<DomainError>, Invoice>(Seq1<DomainError>(new SourceIntegrationEventRequiredError()))
            : Right<Seq<DomainError>, Invoice>(new Invoice(
                InvoiceId.New(),
                orderId,
                customerId,
                amount,
                sourceIntegrationEventId,
                Timestamp.UtcNow(clock)));
    }
}

public sealed record SourceIntegrationEventRequiredError()
    : DomainError(new DomainErrorCode("billing.source_event_required"), "Evento de integração de origem obrigatório.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
