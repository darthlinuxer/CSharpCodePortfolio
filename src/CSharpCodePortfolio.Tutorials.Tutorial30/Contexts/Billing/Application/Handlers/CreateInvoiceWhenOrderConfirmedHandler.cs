using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Handlers;

/// <summary>
/// ACL that translates Ordering's published event into a Billing invoice.
/// </summary>
public sealed class CreateInvoiceWhenOrderConfirmedHandler(
    IInvoiceWriter invoiceWriter,
    ITutorial30UnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<Either<Seq<DomainError>, InvoiceHandlingResult>> HandleAsync(
        OrderConfirmedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var alreadyHandled = await invoiceWriter
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
            Right: tuple => CreateAndCommitAsync(tuple.OrderId, tuple.CustomerId, tuple.Amount, integrationEvent.Id, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, InvoiceHandlingResult>(errors))).ConfigureAwait(false);
    }

    private static Either<Seq<DomainError>, InvoiceValues> CreateValues(OrderConfirmedIntegrationEvent integrationEvent)
    {
        var orderId = BilledOrderId.Create(integrationEvent.OrderId);
        var customerId = BillingCustomerId.Create(integrationEvent.CustomerId);
        var amount = InvoiceAmount.Create(integrationEvent.TotalAmount);
        var errors = Seq(
            ErrorOf(orderId),
            ErrorOf(customerId),
            ErrorOf(amount))
            .Bind(error => error.Match(Some: Seq1, None: Seq<DomainError>));

        return errors.IsEmpty
            ? Right<Seq<DomainError>, InvoiceValues>(new InvoiceValues(GetRight(orderId), GetRight(customerId), GetRight(amount)))
            : Left<Seq<DomainError>, InvoiceValues>(errors);
    }

    private async Task<Either<Seq<DomainError>, InvoiceHandlingResult>> CreateAndCommitAsync(
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
            Right: value => CommitAsync(value, cancellationToken),
            Left: valueErrors => Task.FromResult(Left<Seq<DomainError>, InvoiceHandlingResult>(valueErrors))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, InvoiceHandlingResult>> CommitAsync(
        Invoice invoice,
        CancellationToken cancellationToken)
    {
        invoiceWriter.Add(invoice);
        var commit = await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return commit.Match(
            Right: _ => Right<Seq<DomainError>, InvoiceHandlingResult>(
                new InvoiceHandlingResult.Created(invoice.Id)),
            Left: errors => Left<Seq<DomainError>, InvoiceHandlingResult>(errors));
    }

    private static Option<DomainError> ErrorOf<T>(Either<DomainError, T> result) =>
        result.Match(Right: _ => None, Left: Some);

    private static T GetRight<T>(Either<DomainError, T> result) =>
        result.Match(Right: value => value, Left: error => throw new InvalidOperationException(error.Message));

    private sealed record InvoiceValues(BilledOrderId OrderId, BillingCustomerId CustomerId, InvoiceAmount Amount);
}

public abstract record InvoiceHandlingResult
{
    public sealed record Created(InvoiceId InvoiceId) : InvoiceHandlingResult;

    public sealed record AlreadyHandled(Guid IntegrationEventId) : InvoiceHandlingResult;
}
