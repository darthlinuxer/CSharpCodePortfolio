using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

/// <summary>
/// EF Core context used by the tutorial to prove bounded contexts and outbox in one process.
/// </summary>
public sealed partial class Tutorial30DbContext(DbContextOptions<Tutorial30DbContext> options)
    : DbContext(options)
{
    /// <summary>
    /// Gets the user table used by the registration sample.
    /// </summary>
    public DbSet<UserAccount> Users => Set<UserAccount>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<CustomerDirectoryEntry> CustomerDirectory => Set<CustomerDirectoryEntry>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Applies tutorial mappings that keep persistence concerns outside the aggregate.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Tutorial30DbContext).Assembly);
}
