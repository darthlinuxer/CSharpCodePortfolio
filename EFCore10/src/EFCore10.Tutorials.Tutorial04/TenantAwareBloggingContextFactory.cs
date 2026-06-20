using EFCore10.Tutorials.Tutorial04.Context;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial04;

internal sealed class TenantAwareBloggingContextFactory(IDbContextFactory<BloggingContext> dbContextFactory)
{
    public async Task<TenantBloggingContextLease> CreateDbContextAsync(
        string tenantId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));

        var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        context.SetTenant(tenantId);
        return new TenantBloggingContextLease(context);
    }
}

internal sealed class TenantBloggingContextLease(BloggingContext context) : IAsyncDisposable
{
    public BloggingContext Context => context;

    public async ValueTask DisposeAsync()
    {
        context.ClearTenant();
        await context.DisposeAsync().ConfigureAwait(false);
    }
}
