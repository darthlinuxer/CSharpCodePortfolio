using System.Runtime.CompilerServices;
using EFCore10.Tutorials.Tutorial03.Context;
using EFCore10.Tutorials.Tutorial03.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial03;

public sealed class PooledFactoryDemo(IDbContextFactory<BloggingContext> dbContextFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using (var context = await CreateContextAsync("Setup", cancellationToken).ConfigureAwait(false))
        {
            await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
            await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

            context.Blogs.Add(new Blog
            {
                Url = "https://learn.microsoft.com/ef/core",
                Posts =
                {
                    new Post
                    {
                        Title = "DbContext pooling",
                        Content = "Pooled contexts reduce repeated setup and allocation costs."
                    }
                }
            });

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine("Inserted one blog with one post.");
        }

        await using (var context = await CreateContextAsync("Read", cancellationToken).ConfigureAwait(false))
        {
            var blog = await context.Blogs
                .Include(blog => blog.Posts)
                .OrderBy(blog => blog.BlogId)
                .FirstAsync(cancellationToken).ConfigureAwait(false);

            Console.WriteLine($"Read blog: {blog.Url}");
            Console.WriteLine($"Related posts: {blog.Posts.Count}");
        }

        await using (var context = await CreateContextAsync("Cleanup", cancellationToken).ConfigureAwait(false))
        {
            var blog = await context.Blogs
                .OrderBy(blog => blog.BlogId)
                .FirstAsync(cancellationToken).ConfigureAwait(false);

            context.Blogs.Remove(blog);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine("Deleted the demo blog.");
        }
    }

    private async Task<BloggingContext> CreateContextAsync(
        string operation,
        CancellationToken cancellationToken)
    {
        var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"{operation} DbContext instance: {RuntimeHelpers.GetHashCode(context)}");
        return context;
    }
}
