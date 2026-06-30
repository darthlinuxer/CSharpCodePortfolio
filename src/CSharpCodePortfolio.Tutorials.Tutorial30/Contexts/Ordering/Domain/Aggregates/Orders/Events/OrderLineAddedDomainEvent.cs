using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Events;

public sealed record OrderLineAddedDomainEvent(
    OrderId OrderId,
    Guid OrderLineId,
    Sku Sku,
    Quantity Quantity,
    Money UnitPrice,
    Timestamp OccurredAtUtc)
    : AbstractDomainEvent<Order>("Order line added", OccurredAtUtc);
