using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

/// <summary>
/// EF Core context used by the tutorial to prove async persistence without a real database server.
/// </summary>
public sealed class RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : DbContext(options)
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
}
