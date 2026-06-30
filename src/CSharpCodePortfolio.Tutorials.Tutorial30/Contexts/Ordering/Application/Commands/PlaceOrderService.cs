using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Commands;

/// <summary>
/// Orchestrates order placement without loading Identity's UserAccount aggregate.
/// </summary>
public sealed class PlaceOrderService(
    ICustomerDirectory customerDirectory,
    IOrderWriter orderWriter,
    ITutorial30UnitOfWork unitOfWork,
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
            Right: value => CommitAsync(value, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, PlacedOrderDto>(errors))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, PlacedOrderDto>> CommitAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        orderWriter.Add(order);
        var commit = await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return commit.Match(
            Right: _ => Right<Seq<DomainError>, PlacedOrderDto>(new PlacedOrderDto(
                order.Id.Value,
                order.CustomerId.Value,
                order.Total.Value)),
            Left: errors => Left<Seq<DomainError>, PlacedOrderDto>(errors));
    }

    private static Either<Seq<DomainError>, Seq<OrderLineDraft>> CreateDrafts(
        IReadOnlyCollection<PlaceOrderLineRequest> lines)
    {
        var results = lines.Select(CreateDraft).ToArray();
        var errors = results.SelectMany(ErrorsOf).ToSeq();

        return errors.IsEmpty
            ? Right<Seq<DomainError>, Seq<OrderLineDraft>>(results.SelectMany(DraftsOf).ToSeq())
            : Left<Seq<DomainError>, Seq<OrderLineDraft>>(errors.ToSeq());
    }

    private static Either<Seq<DomainError>, OrderLineDraft> CreateDraft(PlaceOrderLineRequest line)
    {
        var sku = Sku.Create(ToOption(line.Sku));
        var quantity = Quantity.Create(line.Quantity);
        var unitPrice = Money.Create(line.UnitPrice);
        var errors = Seq(
                ErrorOf(sku),
                ErrorOf(quantity),
                ErrorOf(unitPrice))
            .Bind(error => error.Match(Some: Seq1, None: Seq<DomainError>));

        return errors.IsEmpty
            ? Right<Seq<DomainError>, OrderLineDraft>(new OrderLineDraft(GetRight(sku), GetRight(quantity), GetRight(unitPrice)))
            : Left<Seq<DomainError>, OrderLineDraft>(errors);
    }

    private static Either<Seq<DomainError>, Unit> EnsureKnownCustomer(bool exists) =>
        exists
            ? Right<Seq<DomainError>, Unit>(default)
            : Left<Seq<DomainError>, Unit>(Seq1<DomainError>(new UnknownCustomerError()));

    private static Option<DomainError> ErrorOf<T>(Either<DomainError, T> result) =>
        result.Match(Right: _ => None, Left: Some);

    private static IEnumerable<DomainError> ErrorsOf(Either<Seq<DomainError>, OrderLineDraft> result) =>
        result.Match(
            Right: _ => Enumerable.Empty<DomainError>(),
            Left: errors => errors);

    private static IEnumerable<OrderLineDraft> DraftsOf(Either<Seq<DomainError>, OrderLineDraft> result) =>
        result.Match(
            Right: draft => Enumerable.Repeat(draft, 1),
            Left: _ => Enumerable.Empty<OrderLineDraft>());

    private static T GetRight<T>(Either<DomainError, T> result) =>
        result.Match(Right: value => value, Left: error => throw new InvalidOperationException(error.Message));

    private static Option<string> ToOption(string? value) =>
        string.IsNullOrWhiteSpace(value) ? None : Some(value);
}
