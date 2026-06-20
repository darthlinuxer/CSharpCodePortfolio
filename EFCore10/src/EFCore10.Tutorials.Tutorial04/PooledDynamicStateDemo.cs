using EFCore10.Tutorials.Tutorial04.Context;
using EFCore10.Tutorials.Tutorial04.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial04;

internal sealed class PooledDynamicStateDemo(
    IDbContextFactory<BloggingContext> dbContextFactory,
    IDbContextFactory<BadOnConfiguringTenantContext> badTenantContextFactory,
    TenantAwareBloggingContextFactory tenantAwareFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await ResetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await SeedTenantDataAsync(cancellationToken).ConfigureAwait(false);
        await DemonstrateBadOnConfiguringTenantStateAsync(cancellationToken).ConfigureAwait(false);
        await DemonstrateTenantAwareFactorySolutionAsync(cancellationToken).ConfigureAwait(false);
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

    private async Task SeedTenantDataAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        context.Blogs.AddRange(
            new Blog
            {
                TenantId = TenantIds.TenantA,
                Url = "https://tenant-a.example/blog",
                Posts =
                {
                    new Post
                    {
                        Title = "Tenant A post",
                        Content = "This row belongs only to tenant-a."
                    }
                }
            },
            new Blog
            {
                TenantId = TenantIds.TenantB,
                Url = "https://tenant-b.example/blog",
                Posts =
                {
                    new Post
                    {
                        Title = "Tenant B post",
                        Content = "This row belongs only to tenant-b."
                    }
                }
            });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine("Seeded one blog for tenant-a and one blog for tenant-b.");
    }

    private async Task DemonstrateBadOnConfiguringTenantStateAsync(CancellationToken cancellationToken)
    {
        PrintSection("Failure: dynamic state in OnConfiguring");

        try
        {
            AmbientTenantState.CurrentTenantId = TenantIds.TenantA;
            await using (var tenantAContext = await badTenantContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                _ = tenantAContext.Database.ProviderName;
                var tenantABlogCount = await tenantAContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Ambient tenant: tenant-a, context tenant: {tenantAContext.CurrentTenantId}, visible blogs: {tenantABlogCount}");
            }

            AmbientTenantState.CurrentTenantId = TenantIds.TenantB;
            await using (var tenantBContext = await badTenantContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                _ = tenantBContext.Database.ProviderName;
                var tenantBBlogCount = await tenantBContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Ambient tenant: tenant-b, context tenant: {tenantBContext.CurrentTenantId}, visible blogs: {tenantBBlogCount}");

                Console.WriteLine(tenantBContext.CurrentTenantId == TenantIds.TenantB
                    ? "Unexpected: OnConfiguring refreshed the tenant for the reused context."
                    : "Failure: OnConfiguring kept tenant-a while ambient tenant changed to tenant-b");
            }
        }
        finally
        {
            AmbientTenantState.CurrentTenantId = null;
        }
    }

    private async Task DemonstrateTenantAwareFactorySolutionAsync(CancellationToken cancellationToken)
    {
        PrintSection("Solution: tenant-aware factory");

        List<string> tenantABlogs;
        await using (var tenantALease = await tenantAwareFactory
            .CreateDbContextAsync(TenantIds.TenantA, cancellationToken)
            .ConfigureAwait(false))
        {
            tenantABlogs = await ReadBlogUrlsAsync(tenantALease.Context, cancellationToken).ConfigureAwait(false);
        }

        Console.WriteLine($"Tenant-aware factory for tenant-a returned: {FormatUrls(tenantABlogs)}");

        List<string> tenantBBlogs;
        await using (var tenantBLease = await tenantAwareFactory
            .CreateDbContextAsync(TenantIds.TenantB, cancellationToken)
            .ConfigureAwait(false))
        {
            tenantBBlogs = await ReadBlogUrlsAsync(tenantBLease.Context, cancellationToken).ConfigureAwait(false);
        }

        Console.WriteLine($"Tenant-aware factory for tenant-b returned: {FormatUrls(tenantBBlogs)}");

        Console.WriteLine(tenantBBlogs.Count == 1 && tenantBBlogs[0].Contains("tenant-b", StringComparison.Ordinal)
            ? "Solution: tenant-aware factory returned tenant-b data"
            : "Unexpected: tenant-aware factory did not isolate tenant-b data.");
    }

    private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
    {
        PrintSection("Cleanup");

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine("Deleted the demo SQLite database.");
    }

    private static async Task<List<string>> ReadBlogUrlsAsync(
        BloggingContext context,
        CancellationToken cancellationToken)
    {
        return await context.Blogs
            .AsNoTracking()
            .OrderBy(blog => blog.Url)
            .Select(blog => blog.Url)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static string FormatUrls(IReadOnlyCollection<string> urls)
    {
        return urls.Count == 0
            ? "(none)"
            : string.Join(", ", urls);
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"== {title} ==");
    }
}
