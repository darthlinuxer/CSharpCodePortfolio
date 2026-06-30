using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Persistence;

/// <summary>
/// Command-side persistence port for the Order aggregate.
/// </summary>
public interface IOrderWriter
{
    void Add(Order order);

    Task<Option<Order>> FindByIdAsync(OrderId id, CancellationToken cancellationToken);
}
