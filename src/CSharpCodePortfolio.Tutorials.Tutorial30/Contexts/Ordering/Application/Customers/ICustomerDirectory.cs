using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Customers;

/// <summary>
/// Ordering-local customer projection maintained from Identity integration events.
/// </summary>
public interface ICustomerDirectory
{
    Task<bool> ExistsAsync(CustomerId customerId, CancellationToken cancellationToken);

    Task AddIfMissingAsync(CustomerId customerId, string email, CancellationToken cancellationToken);
}
