using System.Data;
using System.Runtime.CompilerServices;
using EFCore10.Tutorials.Tutorial05.Context;
using EFCore10.Tutorials.Tutorial05.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial05;

internal sealed class ConnectionPoolingDemo(IDbContextFactory<PoolingContext> dbContextFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await ResetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await SeedBlogAsync(cancellationToken).ConfigureAwait(false);
        await DemonstrateContextPoolingAsync(cancellationToken).ConfigureAwait(false);
        await DemonstrateConnectionBoundaryAsync(cancellationToken).ConfigureAwait(false);
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

        context.Blogs.Add(new Blog { Url = "https://learn.microsoft.com/ef/core" });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Console.WriteLine("Inserted one blog.");
    }

    private async Task DemonstrateContextPoolingAsync(CancellationToken cancellationToken)
    {
        PrintSection("DbContext pooling reuses context objects");

        var firstHash = await CreateAndReadContextAsync("First context", cancellationToken).ConfigureAwait(false);
        var secondHash = await CreateAndReadContextAsync("Second context", cancellationToken).ConfigureAwait(false);

        Console.WriteLine(firstHash == secondHash
            ? "The same DbContext instance was reused by the EF Core pool."
            : "The pool was allowed to use a different DbContext instance.");
    }

    private async Task DemonstrateConnectionBoundaryAsync(CancellationToken cancellationToken)
    {
        PrintSection("Connection pooling is a different layer");

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var connection = context.Database.GetDbConnection();

        Console.WriteLine($"DbContext instance: {RuntimeHelpers.GetHashCode(context)}");
        Console.WriteLine($"ADO.NET connection state before EF query: {connection.State}");

        var blogCount = await context.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);

        Console.WriteLine($"Read {blogCount} blog(s).");
        Console.WriteLine($"ADO.NET connection state after EF query: {connection.State}");

        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"ADO.NET connection state after explicit OpenConnectionAsync: {connection.State}");

        await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        Console.WriteLine($"ADO.NET connection state after explicit CloseConnectionAsync: {connection.State}");

        Console.WriteLine(connection.State == ConnectionState.Closed
            ? "DbContext pooling is separate from ADO.NET connection pooling."
            : "Unexpected: the ADO.NET connection remained open.");
    }

    private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
    {
        PrintSection("Cleanup");

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine("Deleted the demo SQLite database.");
    }

    private async Task<int> CreateAndReadContextAsync(
        string label,
        CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var blogCount = await context.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
        var hash = RuntimeHelpers.GetHashCode(context);
        Console.WriteLine($"{label}: {hash}, blogs: {blogCount}");
        return hash;
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"== {title} ==");
    }
}
