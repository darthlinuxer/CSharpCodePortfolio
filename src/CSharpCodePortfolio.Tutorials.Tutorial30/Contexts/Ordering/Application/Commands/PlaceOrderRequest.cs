namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Commands;

/// <summary>
/// Command DTO for creating an order from an Ordering customer reference.
/// </summary>
public sealed record PlaceOrderRequest(Guid CustomerId, IReadOnlyCollection<PlaceOrderLineRequest> Lines);

public sealed record PlaceOrderLineRequest(string? Sku, int Quantity, decimal UnitPrice);
