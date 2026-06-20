using Microsoft.EntityFrameworkCore;
using EFCore10.Tutorials.Tutorial02.Models;

namespace EFCore10.Tutorials.Tutorial02.Context;

public class BloggingContext(DbContextOptions<BloggingContext> options) : DbContext(options)
{
    public DbSet<Blog> Blogs { get; set; }

    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);
    }
}
