using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Functional;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;

/// <summary>
/// Ordering aggregate root; references Identity only through CustomerId.
/// </summary>
public sealed class Order : AbstractAggregate<Order, OrderId>
{
    private readonly List<OrderLine> _lines = [];

    private Order()
        : base(OrderId.New())
    {
    }

    private Order(OrderId id, CustomerId customerId, Timestamp placedAtUtc)
        : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Placed;
    }

    public CustomerId CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public Money Total => Money.Sum(_lines.Select(line => line.LineTotal));

    public static Either<Seq<DomainError>, Order> Place(
        CustomerId customerId,
        Seq<OrderLineDraft> lines,
        TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return (!lines.IsEmpty)
            .EnsureSeq(() => new OrderLineRequiredError())
            .Map(_ => CreatePlaced(customerId, lines, clock));
    }

    public Either<DomainError, Unit> AddLine(OrderLineDraft line, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return EnsureCanChange().Bind(_ => AddLineCore(line, clock));
    }

    public Either<DomainError, Unit> RemoveLine(Guid lineId, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return EnsureCanChange()
            .Bind(_ => FindLine(lineId))
            .Bind(line => EnsureCanRemoveLine().Bind(_ => RemoveLineCore(line, clock)));
    }

    public Either<DomainError, Unit> Confirm(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return EnsureNotConfirmed()
            .Bind(_ => EnsureNotCancelled())
            .Bind(_ => ApplyChangeIfDifferent(
                current: Status,
                next: OrderStatus.Confirmed,
                clock,
                apply: value => Status = value,
                createEvent: occurredAtUtc => new OrderConfirmedDomainEvent(Id, CustomerId, Total, occurredAtUtc)));
    }

    public Either<DomainError, Unit> Cancel(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return EnsureNotCancelled()
            .Bind(_ => EnsureNotConfirmed())
            .Bind(_ => ApplyChangeIfDifferent(
                current: Status,
                next: OrderStatus.Cancelled,
                clock,
                apply: value => Status = value,
                createEvent: occurredAtUtc => new OrderCancelledDomainEvent(Id, CustomerId, occurredAtUtc)));
    }

    private static Order CreatePlaced(CustomerId customerId, Seq<OrderLineDraft> lines, TimeProvider clock)
    {
        var placedAtUtc = Timestamp.UtcNow(clock);
        var order = new Order(OrderId.New(), customerId, placedAtUtc);
        order._lines.AddRange(lines.Select(OrderLine.Create));
        order.RecordCreated(placedAtUtc, occurredAtUtc => new OrderPlacedDomainEvent(order.Id, customerId, order.Total, occurredAtUtc));

        return order;
    }

    private Either<DomainError, Unit> AddLineCore(OrderLineDraft line, TimeProvider clock)
    {
        var orderLine = OrderLine.Create(line);
        _lines.Add(orderLine);
        AddDomainEvent(new OrderLineAddedDomainEvent(
            Id,
            orderLine.Id,
            orderLine.Sku,
            orderLine.Quantity,
            orderLine.UnitPrice,
            Timestamp.UtcNow(clock)));

        return Right<DomainError, Unit>(default);
    }

    private Either<DomainError, Unit> RemoveLineCore(OrderLine line, TimeProvider clock)
    {
        _lines.Remove(line);
        AddDomainEvent(new OrderLineRemovedDomainEvent(
            Id,
            line.Id,
            line.Sku,
            line.Quantity,
            line.UnitPrice,
            Timestamp.UtcNow(clock)));

        return Right<DomainError, Unit>(default);
    }

    private Either<DomainError, Unit> EnsureCanChange() =>
        (Status == OrderStatus.Placed).Ensure(() => new OrderCannotBeChangedError(Status));

    private Either<DomainError, Unit> EnsureCanRemoveLine() =>
        (_lines.Count > 1).Ensure(() => new OrderLineRequiredError());

    private Either<DomainError, Unit> EnsureNotConfirmed() =>
        (Status != OrderStatus.Confirmed).Ensure(() => new OrderAlreadyConfirmedError());

    private Either<DomainError, Unit> EnsureNotCancelled() =>
        (Status != OrderStatus.Cancelled).Ensure(() => new OrderAlreadyCancelledError());

    private Either<DomainError, OrderLine> FindLine(Guid lineId) =>
        Optional(_lines.Find(line => line.Id == lineId))
            .ToEither<DomainError>(new OrderLineNotFoundError());
}
