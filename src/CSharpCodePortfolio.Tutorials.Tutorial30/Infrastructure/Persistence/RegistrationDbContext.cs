using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

/// <summary>
/// EF Core context used by the tutorial to prove async persistence without a real database server.
/// </summary>
public sealed class RegistrationDbContext(DbContextOptions<RegistrationDbContext> options)
    : DbContext(options), IRegistrationUnitOfWork
{
    /// <summary>
    /// Gets the user table used by the registration sample.
    /// </summary>
    public DbSet<UserAccount> Users => Set<UserAccount>();

    /// <summary>
    /// Applies tutorial mappings that keep persistence concerns outside the aggregate.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegistrationDbContext).Assembly);

    /// <summary>
    /// Commits tracked changes and clears domain events only after EF Core persists successfully.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<IEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => !entity.DomainEvents.IsEmpty)
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return result;
    }
}
