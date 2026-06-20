using EFCore10.Tutorials.Tutorial04.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial04.Context;

public sealed class BloggingContext(DbContextOptions<BloggingContext> options) : DbContext(options)
{
    public string CurrentTenantId { get; private set; } = string.Empty;

    public DbSet<Blog> Blogs => Set<Blog>();

    public DbSet<Post> Posts => Set<Post>();

    public void SetTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));

        CurrentTenantId = tenantId;
    }

    public void ClearTenant()
    {
        CurrentTenantId = string.Empty;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);
        modelBuilder.Entity<Blog>()
            .HasQueryFilter(blog => blog.TenantId == CurrentTenantId);
    }
}
