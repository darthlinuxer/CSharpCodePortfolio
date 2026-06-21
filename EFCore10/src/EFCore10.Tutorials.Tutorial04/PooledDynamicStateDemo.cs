using System.Runtime.CompilerServices;
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
        TutorialConsole.WriteCodeSnippet(
            "Exemplo ruim: OnConfiguring captura o tenant só quando a instância é inicializada.",
            "Context/BadOnConfiguringTenantContext.cs",
            """
            protected override void OnConfiguring(
                DbContextOptionsBuilder optionsBuilder)
            {
                CurrentTenantId =
                    AmbientTenantState.CurrentTenantId ?? string.Empty;
            }
            """);

        try
        {
            AmbientTenantState.CurrentTenantId = TenantIds.TenantA;
            int tenantAHash;
            string tenantAContextTenant;
            List<string> tenantAUrls;
            await using (var tenantAContext = await badTenantContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                _ = tenantAContext.Database.ProviderName;
                tenantAHash = RuntimeHelpers.GetHashCode(tenantAContext);
                tenantAContextTenant = tenantAContext.CurrentTenantId;
                tenantAUrls = await ReadBlogUrlsAsync(tenantAContext, cancellationToken).ConfigureAwait(false);
            }

            AmbientTenantState.CurrentTenantId = TenantIds.TenantB;
            int tenantBHash;
            string tenantBContextTenant;
            List<string> tenantBUrls;
            await using (var tenantBContext = await badTenantContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                _ = tenantBContext.Database.ProviderName;
                tenantBHash = RuntimeHelpers.GetHashCode(tenantBContext);
                tenantBContextTenant = tenantBContext.CurrentTenantId;
                tenantBUrls = await ReadBlogUrlsAsync(tenantBContext, cancellationToken).ConfigureAwait(false);
                var tenantWasUpdated = tenantBContext.CurrentTenantId == TenantIds.TenantB;

                TutorialConsole.WriteEvidence(
                    "OnConfiguring com estado dinâmico",
                    ("Pool size", PooledDynamicStateTutorial.DemoPoolSize.ToString()),
                    ("Hash primeiro uso", tenantAHash.ToString()),
                    ("Tenant ambiente no primeiro uso", TenantIds.TenantA),
                    ("Tenant guardado no primeiro contexto", tenantAContextTenant),
                    ("URLs visíveis no primeiro uso", FormatUrls(tenantAUrls)),
                    ("Hash segundo uso", tenantBHash.ToString()),
                    ("Tenant ambiente no segundo uso", TenantIds.TenantB),
                    ("Tenant guardado no segundo contexto", tenantBContextTenant),
                    ("URLs visíveis no segundo uso", FormatUrls(tenantBUrls)),
                    ("Mesmo objeto CLR?", FormatBoolean(tenantAHash == tenantBHash)),
                    ("Tenant atualizado no segundo uso?", FormatBoolean(tenantWasUpdated)));

                TutorialConsole.WriteConclusion(
                    tenantWasUpdated
                        ? "Resultado inesperado: OnConfiguring atualizou o tenant no contexto reaproveitado."
                        : "Falha demonstrada: OnConfiguring manteve tenant-a enquanto o tenant ambiente mudou para tenant-b.",
                    tenantWasUpdated
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
        TutorialConsole.WriteCodeSnippet(
            "O contexto correto expõe métodos explícitos para aplicar e limpar o tenant.",
            "Context/BloggingContext.cs",
            """
            public void SetTenant(string tenantId)
            {
                CurrentTenantId = tenantId;
            }

            public void ClearTenant()
            {
                CurrentTenantId = string.Empty;
            }
            """);
        TutorialConsole.WriteCodeSnippet(
            "O filtro global depende do tenant aplicado na instância atual.",
            "Context/BloggingContext.cs",
            """
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasQueryFilter(blog => blog.TenantId == CurrentTenantId);
            }
            """);
        TutorialConsole.WriteCodeSnippet(
            "A factory aplica o tenant em cada lease e limpa antes de devolver ao pool.",
            "TenantAwareBloggingContextFactory.cs",
            """
            var context =
                await dbContextFactory.CreateDbContextAsync(cancellationToken);
            context.SetTenant(tenantId);
            return new TenantBloggingContextLease(context);

            public async ValueTask DisposeAsync()
            {
                context.ClearTenant();
                await context.DisposeAsync();
            }
            """);

        List<string> tenantABlogs;
        string tenantAContextTenant;
        int tenantAHash;
        await using (var tenantALease = await tenantAwareFactory
            .CreateDbContextAsync(TenantIds.TenantA, cancellationToken)
            .ConfigureAwait(false))
        {
            tenantAHash = RuntimeHelpers.GetHashCode(tenantALease.Context);
            tenantAContextTenant = tenantALease.Context.CurrentTenantId;
            tenantABlogs = await ReadBlogUrlsAsync(tenantALease.Context, cancellationToken).ConfigureAwait(false);
        }

        List<string> tenantBBlogs;
        string tenantBContextTenant;
        int tenantBHash;
        await using (var tenantBLease = await tenantAwareFactory
            .CreateDbContextAsync(TenantIds.TenantB, cancellationToken)
            .ConfigureAwait(false))
        {
            tenantBHash = RuntimeHelpers.GetHashCode(tenantBLease.Context);
            tenantBContextTenant = tenantBLease.Context.CurrentTenantId;
            tenantBBlogs = await ReadBlogUrlsAsync(tenantBLease.Context, cancellationToken).ConfigureAwait(false);
        }

        string tenantAfterLease;
        await using (var rawContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            tenantAfterLease = rawContext.CurrentTenantId;
        }

        var tenantBWasIsolated = tenantBBlogs.Count == 1
            && tenantBBlogs[0].Contains("tenant-b", StringComparison.Ordinal);

        TutorialConsole.WriteEvidence(
            "Factory tenant-aware",
            ("Pool size", PooledDynamicStateTutorial.DemoPoolSize.ToString()),
            ("Tenant solicitado A", TenantIds.TenantA),
            ("Tenant aplicado no contexto A", tenantAContextTenant),
            ("URLs retornadas para A", FormatUrls(tenantABlogs)),
            ("Tenant solicitado B", TenantIds.TenantB),
            ("Tenant aplicado no contexto B", tenantBContextTenant),
            ("URLs retornadas para B", FormatUrls(tenantBBlogs)),
            ("Mesmo objeto CLR?", FormatBoolean(tenantAHash == tenantBHash)),
            ("Tenant após descarte do lease", string.IsNullOrEmpty(tenantAfterLease) ? "(vazio)" : tenantAfterLease),
            ("Tenant B isolado?", FormatBoolean(tenantBWasIsolated)));

        TutorialConsole.WriteConclusion(
            tenantBWasIsolated
                ? "Solução demonstrada: a factory tenant-aware retornou dados de tenant-b quando o lease foi criado para tenant-b."
                : "Resultado inesperado: a factory tenant-aware não isolou os dados de tenant-b.",
            tenantBWasIsolated
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

    private static async Task<List<string>> ReadBlogUrlsAsync(
        BadOnConfiguringTenantContext context,
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

    private static string FormatBoolean(bool value)
    {
        return value ? "sim" : "não";
    }

}
