using EFCore10.Tutorials.Tutorial04.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial04.Context;

internal sealed class BadOnConfiguringTenantContext(DbContextOptions<BadOnConfiguringTenantContext> options) : DbContext(options)
{
    public string CurrentTenantId { get; private set; } = string.Empty;

    public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        CurrentTenantId = AmbientTenantState.CurrentTenantId ?? string.Empty;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);
        modelBuilder.Entity<Blog>()
            .HasQueryFilter(blog => blog.TenantId == CurrentTenantId);
    }
}
