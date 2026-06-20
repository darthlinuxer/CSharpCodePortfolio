using System.Runtime.CompilerServices;
using EFCore10.Tutorials.Tutorial03.Context;
using EFCore10.Tutorials.Tutorial03.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial03;

internal sealed class PooledFactoryDemo(IDbContextFactory<BloggingContext> dbContextFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await ResetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await SeedBlogAsync(cancellationToken).ConfigureAwait(false);
        await DemonstrateDisposedContextReuseAsync(cancellationToken).ConfigureAwait(false);
        await DemonstrateUndisposedContextFailureAsync(cancellationToken).ConfigureAwait(false);
        await CleanupDatabaseAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        PrintSection("Setup");

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        Console.WriteLine("Database schema was recreated.");
    }

    private async Task SeedBlogAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

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

    private async Task DemonstrateDisposedContextReuseAsync(CancellationToken cancellationToken)
    {
        PrintSection("Disposed contexts return to the pool");

        var firstHash = await CreateReadAndDisposeContextAsync("First disposed context", cancellationToken)
            .ConfigureAwait(false);
        var secondHash = await CreateReadAndDisposeContextAsync("Second disposed context", cancellationToken)
            .ConfigureAwait(false);

        Console.WriteLine(firstHash == secondHash
            ? "Disposed context returned to the pool and the same instance was reused."
            : "The pool created a different instance, but both contexts were disposed correctly.");
    }

    private async Task DemonstrateUndisposedContextFailureAsync(CancellationToken cancellationToken)
    {
        PrintSection("A checked-out context cannot be reused by the pool");

        BloggingContext? leakedContext = null;

        try
        {
            leakedContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            _ = await leakedContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
            var leakedHash = RuntimeHelpers.GetHashCode(leakedContext);
            Console.WriteLine($"Leaked context instance: {leakedHash}");

            await using var replacementContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            _ = await replacementContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
            var replacementHash = RuntimeHelpers.GetHashCode(replacementContext);
            Console.WriteLine($"Context requested while leaked one is still checked out: {replacementHash}");

            Console.WriteLine(replacementHash == leakedHash
                ? "Unexpected: the checked-out context was reused."
                : "Failure: context was not returned to the pool because it was not disposed");
        }
        finally
        {
            if (leakedContext is not null)
            {
                await leakedContext.DisposeAsync().ConfigureAwait(false);
                Console.WriteLine("The leaked context was disposed in cleanup.");
            }
        }
    }

    private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
    {
        PrintSection("Cleanup");

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine("Deleted the demo SQLite database.");
    }

    private async Task<int> CreateReadAndDisposeContextAsync(
        string label,
        CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var postCount = await context.Posts.CountAsync(cancellationToken).ConfigureAwait(false);
        var hash = RuntimeHelpers.GetHashCode(context);
        Console.WriteLine($"{label}: {hash}, posts: {postCount}");
        return hash;
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"== {title} ==");
    }
}
