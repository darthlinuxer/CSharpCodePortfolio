using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Customers;

/// <summary>
/// EF Core adapter for Ordering's local customer directory.
/// </summary>
public sealed class EfCustomerDirectory(Tutorial30DbContext dbContext) : ICustomerDirectory
{
    public Task<bool> ExistsAsync(CustomerId customerId, CancellationToken cancellationToken) =>
        dbContext.CustomerDirectory.AsNoTracking().AnyAsync(customer => customer.Id == customerId, cancellationToken);

    public async Task AddIfMissingAsync(CustomerId customerId, string email, CancellationToken cancellationToken)
    {
        var exists = await ExistsAsync(customerId, cancellationToken).ConfigureAwait(false);
        _ = exists ? default : Add(customerId, email);
    }

    private Unit Add(CustomerId customerId, string email)
    {
        dbContext.CustomerDirectory.Add(new CustomerDirectoryEntry(customerId, email));
        return default;
    }
}
