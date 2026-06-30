namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;

/// <summary>
/// Lifecycle states protected by the Order aggregate.
/// </summary>
public enum OrderStatus
{
    Placed = 0,
    Confirmed = 1,
    Cancelled = 2
}
