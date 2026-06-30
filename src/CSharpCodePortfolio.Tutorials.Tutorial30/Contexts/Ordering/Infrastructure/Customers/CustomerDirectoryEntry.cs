using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Customers;

/// <summary>
/// Ordering-owned customer projection fed by Identity integration events.
/// </summary>
public sealed class CustomerDirectoryEntry
{
    private CustomerDirectoryEntry()
    {
        Email = string.Empty;
    }

    public CustomerDirectoryEntry(CustomerId id, string email)
    {
        Id = id;
        Email = email;
    }

    public CustomerId Id { get; private set; }

    public string Email { get; private set; }
}
