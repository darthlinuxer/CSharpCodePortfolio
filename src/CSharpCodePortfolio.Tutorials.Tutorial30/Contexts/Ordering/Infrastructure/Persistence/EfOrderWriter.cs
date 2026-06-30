using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Persistence;

/// <summary>
/// EF Core adapter for command-side Order persistence.
/// </summary>
public sealed class EfOrderWriter(Tutorial30DbContext dbContext) : IOrderWriter
{
    public void Add(Order order)
    {
        dbContext.Orders.Add(order);
    }

    public async Task<Option<Order>> FindByIdAsync(OrderId id, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .Include(order => order.Lines)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return order is null ? None : Some(order);
    }
}
