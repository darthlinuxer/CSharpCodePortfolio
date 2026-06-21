using System.Runtime.CompilerServices;
using EFCore10.Shared;
using EFCore10.Tutorials.Tutorial03.Context;
using EFCore10.Tutorials.Tutorial03.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial03;

internal sealed class PooledFactoryDemo(IDbContextFactory<BloggingContext> dbContextFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteQuestion(
            "O que acontece com um contexto criado por IDbContextFactory quando ele é descartado?");
        TutorialConsole.WriteHypothesis(
            "Com pool size 1, um contexto descartado pode voltar ao pool.",
            "Um contexto não descartado fica emprestado e força a factory a entregar outra instância.");
        TutorialConsole.WriteCodeSnippet(
            "Registrar uma factory pooled com pool size controlado pelo tutorial.",
            "Tutorial03.cs",
            """
            services.AddSqlitePooledDbContextFactory<BloggingContext>(
                connectionString,
                DemoPoolSize);
            services.AddScoped<PooledFactoryDemo>();
            """);

        await ResetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await SeedBlogAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WritePreparation(
            "O schema do banco foi recriado.",
            "Foi inserido um blog com um post para que as consultas executem trabalho real.");

        await DemonstrateDisposedContextReuseAsync(cancellationToken).ConfigureAwait(false);
        await DemonstrateUndisposedContextFailureAsync(cancellationToken).ConfigureAwait(false);
        await CleanupDatabaseAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
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
    }

    private async Task DemonstrateDisposedContextReuseAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteExperiment(
            1,
            "Contexto descartado volta ao pool?",
            "Criar, consultar e descartar dois contextos em sequência usando uma factory pooled com pool size 1.");
        TutorialConsole.WriteCodeSnippet(
            "Criar o DbContext com await using para devolvê-lo ao pool no descarte.",
            "PooledFactoryDemo.cs",
            """
            await using var context =
                await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var postCount = await context.Posts.CountAsync(cancellationToken);
            var hash = RuntimeHelpers.GetHashCode(context);
            """);

        var firstContext = await CreateReadAndDisposeContextAsync(cancellationToken)
            .ConfigureAwait(false);
        var secondContext = await CreateReadAndDisposeContextAsync(cancellationToken)
            .ConfigureAwait(false);
        var sameInstance = firstContext.Hash == secondContext.Hash;

        TutorialConsole.WriteEvidence(
            "Contextos descartados",
            ("Pool size", Tutorial03.DemoPoolSize.ToString()),
            ("Hash primeiro contexto", firstContext.Hash.ToString()),
            ("Posts lidos no primeiro", firstContext.PostCount.ToString()),
            ("Hash segundo contexto", secondContext.Hash.ToString()),
            ("Posts lidos no segundo", secondContext.PostCount.ToString()),
            ("Mesmo objeto CLR?", FormatBoolean(sameInstance)));

        TutorialConsole.WriteConclusion(
            sameInstance
                ? "O hash se repetiu; isso mostra que a primeira instância foi descartada, voltou ao pool e foi reutilizada."
                : "O hash mudou; isso ainda é válido, mas neste teste o pool entregou outra instância mesmo com os contextos descartados.",
            sameInstance ? TutorialConclusionKind.Success : TutorialConclusionKind.Warning);
    }

    private async Task DemonstrateUndisposedContextFailureAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteExperiment(
            2,
            "Contexto não descartado volta ao pool?",
            "Manter um contexto sem descarte e pedir outro contexto à factory enquanto o primeiro ainda está emprestado.");
        TutorialConsole.WriteCodeSnippet(
            "Manter um contexto emprestado impede a reutilização imediata daquela instância.",
            "PooledFactoryDemo.cs",
            """
            leakedContext =
                await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var leakedHash = RuntimeHelpers.GetHashCode(leakedContext);

            await using var replacementContext =
                await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var replacementHash = RuntimeHelpers.GetHashCode(replacementContext);
            """);

        BloggingContext? leakedContext = null;

        try
        {
            leakedContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var leakedBlogCount = await leakedContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
            var leakedHash = RuntimeHelpers.GetHashCode(leakedContext);

            await using var replacementContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var replacementBlogCount = await replacementContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
            var replacementHash = RuntimeHelpers.GetHashCode(replacementContext);
            var sameInstance = replacementHash == leakedHash;

            TutorialConsole.WriteEvidence(
                "Contexto não descartado",
                ("Pool size", Tutorial03.DemoPoolSize.ToString()),
                ("Hash contexto emprestado", leakedHash.ToString()),
                ("Blogs lidos no contexto emprestado", leakedBlogCount.ToString()),
                ("Hash contexto substituto", replacementHash.ToString()),
                ("Blogs lidos no substituto", replacementBlogCount.ToString()),
                ("Mesmo objeto CLR?", FormatBoolean(sameInstance)),
                ("Interpretação", sameInstance ? "falha: instância emprestada foi reutilizada" : "evidência: a instância emprestada ficou fora do pool"));

            TutorialConsole.WriteConclusion(
                sameInstance
                    ? "Resultado inesperado: o mesmo contexto ainda emprestado foi reutilizado."
                    : "O contexto não descartado não voltou ao pool; por isso a factory precisou entregar outra instância.",
                sameInstance ? TutorialConclusionKind.Failure : TutorialConclusionKind.Warning);
        }
        finally
        {
            if (leakedContext is not null)
            {
                await leakedContext.DisposeAsync().ConfigureAwait(false);
                TutorialConsole.WriteObservation("O contexto mantido aberto foi descartado na limpeza do experimento.");
            }
        }
    }

    private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteCleanup("O banco SQLite de demonstração foi removido.");
    }

    private async Task<(int Hash, int PostCount)> CreateReadAndDisposeContextAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var postCount = await context.Posts.CountAsync(cancellationToken).ConfigureAwait(false);
        var hash = RuntimeHelpers.GetHashCode(context);
        return (hash, postCount);
    }

    private static string FormatBoolean(bool value)
    {
        return value ? "sim" : "não";
    }
}
