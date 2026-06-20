using EFCore10.Tutorials.Tutorial03.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial03.Context;

public sealed class BloggingContext(DbContextOptions<BloggingContext> options) : DbContext(options)
{
    public DbSet<Blog> Blogs => Set<Blog>();

    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);
    }
}
