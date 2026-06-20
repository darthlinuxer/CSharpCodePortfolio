using EFCore10.Tutorials.Tutorial05.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial05.Context;

public sealed class PoolingContext(DbContextOptions<PoolingContext> options) : DbContext(options)
{
    public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PoolingContext).Assembly);
    }
}
