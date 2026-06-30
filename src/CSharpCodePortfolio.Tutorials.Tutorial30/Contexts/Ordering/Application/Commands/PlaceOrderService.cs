using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Functional;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Commands;

/// <summary>
/// Orchestrates order placement without loading Identity's UserAccount aggregate.
/// </summary>
public sealed class PlaceOrderService(
    ICustomerDirectory customerDirectory,
    IRepository<Order, OrderId> repository,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<Either<Seq<DomainError>, PlacedOrderDto>> PlaceAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await CustomerId.Create(request.CustomerId).Match(
            Right: customerId => PlaceForCustomerAsync(customerId, request.Lines, cancellationToken),
            Left: error => Task.FromResult(Left<Seq<DomainError>, PlacedOrderDto>(Seq1(error)))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, PlacedOrderDto>> PlaceForCustomerAsync(
        CustomerId customerId,
        IReadOnlyCollection<PlaceOrderLineRequest> lines,
        CancellationToken cancellationToken)
    {
        var exists = await customerDirectory.ExistsAsync(customerId, cancellationToken).ConfigureAwait(false);

        return await EnsureKnownCustomer(exists).Match(
            Right: _ => CreateDrafts(lines).Match(
                Right: validLines => PersistAsync(customerId, validLines, cancellationToken),
                Left: errors => Task.FromResult(Left<Seq<DomainError>, PlacedOrderDto>(errors))),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, PlacedOrderDto>(errors))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, PlacedOrderDto>> PersistAsync(
        CustomerId customerId,
        Seq<OrderLineDraft> lines,
        CancellationToken cancellationToken)
    {
        var order = Order.Place(customerId, lines, clock);
        return await order.Match(
            Right: value => SaveAsync(value, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, PlacedOrderDto>(errors))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, PlacedOrderDto>> SaveAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        repository.Add(order);
        var saved = await unitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

        return saved.Match(
            Right: _ => Right<Seq<DomainError>, PlacedOrderDto>(new PlacedOrderDto(
                order.Id.Value,
                order.CustomerId.Value,
                order.Total.Value)),
            Left: errors => Left<Seq<DomainError>, PlacedOrderDto>(errors));
    }

    private static Either<Seq<DomainError>, Seq<OrderLineDraft>> CreateDrafts(
        IReadOnlyCollection<PlaceOrderLineRequest> lines) =>
        lines.Select(CreateDraft).Collect();

    private static Either<Seq<DomainError>, OrderLineDraft> CreateDraft(PlaceOrderLineRequest line)
    {
        return (
            Sku.Create(line.Sku.ToNonBlankOption()),
            Quantity.Create(line.Quantity),
            Money.Create(line.UnitPrice))
            .Combine((sku, quantity, unitPrice) => new OrderLineDraft(sku, quantity, unitPrice));
    }

    private static Either<Seq<DomainError>, Unit> EnsureKnownCustomer(bool exists) =>
        exists.EnsureSeq(() => new UnknownCustomerError());
}
