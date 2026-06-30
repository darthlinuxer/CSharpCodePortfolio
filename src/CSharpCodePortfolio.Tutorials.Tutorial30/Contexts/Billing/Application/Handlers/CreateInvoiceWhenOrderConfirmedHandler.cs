using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Messaging;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Functional;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Handlers;

/// <summary>
/// ACL that translates Ordering's published event into a Billing invoice.
/// </summary>
public sealed class CreateInvoiceWhenOrderConfirmedHandler(
    IInvoiceLookup invoiceLookup,
    IRepository<Invoice, InvoiceId> repository,
    IUnitOfWork unitOfWork,
    TimeProvider clock) : IIntegrationEventHandler<OrderConfirmedIntegrationEvent, InvoiceHandlingResult>
{
    public async Task<Either<Seq<DomainError>, InvoiceHandlingResult>> HandleAsync(
        OrderConfirmedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var alreadyHandled = await invoiceLookup
            .ExistsForIntegrationEventAsync(integrationEvent.Id, cancellationToken)
            .ConfigureAwait(false);

        return alreadyHandled
            ? Right<Seq<DomainError>, InvoiceHandlingResult>(
                new InvoiceHandlingResult.AlreadyHandled(integrationEvent.Id))
            : await CreateInvoiceAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, InvoiceHandlingResult>> CreateInvoiceAsync(
        OrderConfirmedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var values = CreateValues(integrationEvent);
        return await values.Match(
            Right: tuple => CreateAndSaveAsync(tuple.OrderId, tuple.CustomerId, tuple.Amount, integrationEvent.Id, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, InvoiceHandlingResult>(errors))).ConfigureAwait(false);
    }

    private static Either<Seq<DomainError>, InvoiceValues> CreateValues(OrderConfirmedIntegrationEvent integrationEvent)
    {
        var orderId = BilledOrderId.Create(integrationEvent.OrderId);
        var customerId = BillingCustomerId.Create(integrationEvent.CustomerId);
        var amount = InvoiceAmount.Create(integrationEvent.TotalAmount);

        return (orderId, customerId, amount)
            .Combine((validOrderId, validCustomerId, validAmount) =>
                new InvoiceValues(validOrderId, validCustomerId, validAmount));
    }

    private async Task<Either<Seq<DomainError>, InvoiceHandlingResult>> CreateAndSaveAsync(
        BilledOrderId orderId,
        BillingCustomerId customerId,
        InvoiceAmount amount,
        Guid integrationEventId,
        CancellationToken cancellationToken)
    {
        var invoice = Invoice.Create(
            orderId,
            customerId,
            amount,
            integrationEventId,
            clock);

        return await invoice.Match(
            Right: value => SaveAsync(value, cancellationToken),
            Left: valueErrors => Task.FromResult(Left<Seq<DomainError>, InvoiceHandlingResult>(valueErrors))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, InvoiceHandlingResult>> SaveAsync(
        Invoice invoice,
        CancellationToken cancellationToken)
    {
        repository.Add(invoice);
        var saved = await unitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

        return saved.Match(
            Right: _ => Right<Seq<DomainError>, InvoiceHandlingResult>(
                new InvoiceHandlingResult.Created(invoice.Id)),
            Left: errors => Left<Seq<DomainError>, InvoiceHandlingResult>(errors));
    }

    private sealed record InvoiceValues(BilledOrderId OrderId, BillingCustomerId CustomerId, InvoiceAmount Amount);
}

public abstract record InvoiceHandlingResult
{
    public sealed record Created(InvoiceId InvoiceId) : InvoiceHandlingResult;

    public sealed record AlreadyHandled(Guid IntegrationEventId) : InvoiceHandlingResult;
}
