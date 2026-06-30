using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;

/// <summary>
/// Validated line data used to create an OrderLine inside the aggregate.
/// </summary>
public sealed record OrderLineDraft(Sku Sku, Quantity Quantity, Money UnitPrice);
