namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Commands;

/// <summary>
/// Command DTO for confirming an order.
/// </summary>
public sealed record ConfirmOrderRequest(Guid OrderId);
