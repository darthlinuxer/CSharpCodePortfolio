using EFCore10.Shared;
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
        TutorialConsole.WriteQuestion(
            "Por que estado dinâmico, como tenant, não deve ser configurado em OnConfiguring com contexto pooled?");
        TutorialConsole.WriteHypothesis(
            "Como o mesmo DbContext pode voltar do pool, estado que muda por operação precisa ser aplicado em cada lease.",
            "Se esse estado for carregado só na inicialização da instância, ele pode ficar obsoleto quando o contexto for reaproveitado.");

        await ResetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await SeedTenantDataAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WritePreparation(
            "O schema do banco foi recriado.",
            "Foram inseridos um blog para tenant-a e um blog para tenant-b.");

        await DemonstrateBadOnConfiguringTenantStateAsync(cancellationToken).ConfigureAwait(false);
        await DemonstrateTenantAwareFactorySolutionAsync(cancellationToken).ConfigureAwait(false);
        await CleanupDatabaseAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
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
    }

    private async Task DemonstrateBadOnConfiguringTenantStateAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteExperiment(
            1,
            "OnConfiguring mantém estado dinâmico obsoleto?",
            "Usar um contexto ruim que lê o tenant ambiente em OnConfiguring; depois mudar o ambiente de tenant-a para tenant-b.");

        try
        {
            AmbientTenantState.CurrentTenantId = TenantIds.TenantA;
            await using (var tenantAContext = await badTenantContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                _ = tenantAContext.Database.ProviderName;
                var tenantABlogCount = await tenantAContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
                TutorialConsole.WriteObservation(
                    $"Primeiro uso: tenant ambiente tenant-a, tenant guardado no contexto {tenantAContext.CurrentTenantId}, blogs visíveis {tenantABlogCount}.");
            }

            AmbientTenantState.CurrentTenantId = TenantIds.TenantB;
            await using (var tenantBContext = await badTenantContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                _ = tenantBContext.Database.ProviderName;
                var tenantBBlogCount = await tenantBContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
                TutorialConsole.WriteObservation(
                    $"Segundo uso: tenant ambiente tenant-b, tenant guardado no contexto {tenantBContext.CurrentTenantId}, blogs visíveis {tenantBBlogCount}.");

                TutorialConsole.WriteConclusion(
                    tenantBContext.CurrentTenantId == TenantIds.TenantB
                        ? "Resultado inesperado: OnConfiguring atualizou o tenant no contexto reaproveitado."
                        : "Falha demonstrada: OnConfiguring manteve tenant-a enquanto o tenant ambiente mudou para tenant-b.",
                    tenantBContext.CurrentTenantId == TenantIds.TenantB
                        ? TutorialConclusionKind.Warning
                        : TutorialConclusionKind.Failure);
            }
        }
        finally
        {
            AmbientTenantState.CurrentTenantId = null;
        }
    }

    private async Task DemonstrateTenantAwareFactorySolutionAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteExperiment(
            2,
            "Factory tenant-aware aplica o tenant por lease?",
            "Usar TenantAwareBloggingContextFactory para aplicar o tenant no início de cada lease e limpar o estado no descarte.");

        List<string> tenantABlogs;
        await using (var tenantALease = await tenantAwareFactory
            .CreateDbContextAsync(TenantIds.TenantA, cancellationToken)
            .ConfigureAwait(false))
        {
            tenantABlogs = await ReadBlogUrlsAsync(tenantALease.Context, cancellationToken).ConfigureAwait(false);
        }

        TutorialConsole.WriteObservation(
            $"Factory tenant-aware para tenant-a retornou: {FormatUrls(tenantABlogs)}.");

        List<string> tenantBBlogs;
        await using (var tenantBLease = await tenantAwareFactory
            .CreateDbContextAsync(TenantIds.TenantB, cancellationToken)
            .ConfigureAwait(false))
        {
            tenantBBlogs = await ReadBlogUrlsAsync(tenantBLease.Context, cancellationToken).ConfigureAwait(false);
        }

        TutorialConsole.WriteObservation(
            $"Factory tenant-aware para tenant-b retornou: {FormatUrls(tenantBBlogs)}.");

        TutorialConsole.WriteConclusion(
            tenantBBlogs.Count == 1 && tenantBBlogs[0].Contains("tenant-b", StringComparison.Ordinal)
                ? "Solução demonstrada: a factory tenant-aware retornou dados de tenant-b quando o lease foi criado para tenant-b."
                : "Resultado inesperado: a factory tenant-aware não isolou os dados de tenant-b.",
            tenantBBlogs.Count == 1 && tenantBBlogs[0].Contains("tenant-b", StringComparison.Ordinal)
                ? TutorialConclusionKind.Success
                : TutorialConclusionKind.Failure);
    }

    private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteCleanup("O banco SQLite de demonstração foi removido.");
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
            ? "(nenhum)"
            : string.Join(", ", urls);
    }

}
