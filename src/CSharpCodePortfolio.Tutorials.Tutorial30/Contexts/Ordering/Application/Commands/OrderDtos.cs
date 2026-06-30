namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Commands;

public sealed record PlacedOrderDto(Guid Id, Guid CustomerId, decimal TotalAmount);

public sealed record ConfirmedOrderDto(Guid Id, Guid CustomerId, decimal TotalAmount);
