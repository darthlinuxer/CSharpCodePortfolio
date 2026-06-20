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

        var firstHash = await CreateReadAndDisposeContextAsync("Primeiro contexto descartado", cancellationToken)
            .ConfigureAwait(false);
        var secondHash = await CreateReadAndDisposeContextAsync("Segundo contexto descartado", cancellationToken)
            .ConfigureAwait(false);

        TutorialConsole.WriteConclusion(
            firstHash == secondHash
                ? "O hash se repetiu; isso mostra que a primeira instância foi descartada, voltou ao pool e foi reutilizada."
                : "O hash mudou; isso ainda é válido, mas neste teste o pool entregou outra instância mesmo com os contextos descartados.",
            firstHash == secondHash ? TutorialConclusionKind.Success : TutorialConclusionKind.Warning);
    }

    private async Task DemonstrateUndisposedContextFailureAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteExperiment(
            2,
            "Contexto não descartado volta ao pool?",
            "Manter um contexto sem descarte e pedir outro contexto à factory enquanto o primeiro ainda está emprestado.");

        BloggingContext? leakedContext = null;

        try
        {
            leakedContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            _ = await leakedContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
            var leakedHash = RuntimeHelpers.GetHashCode(leakedContext);
            TutorialConsole.WriteObservation(
                $"Contexto ainda não descartado: hash {leakedHash}. Ele continua fora do pool.");

            await using var replacementContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            _ = await replacementContext.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
            var replacementHash = RuntimeHelpers.GetHashCode(replacementContext);
            TutorialConsole.WriteObservation(
                $"Contexto entregue enquanto o primeiro ainda está emprestado: hash {replacementHash}.");

            TutorialConsole.WriteConclusion(
                replacementHash == leakedHash
                    ? "Resultado inesperado: o mesmo contexto ainda emprestado foi reutilizado."
                    : "O contexto não descartado não voltou ao pool; por isso a factory precisou entregar outra instância.",
                replacementHash == leakedHash ? TutorialConclusionKind.Failure : TutorialConclusionKind.Warning);
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

    private async Task<int> CreateReadAndDisposeContextAsync(
        string label,
        CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var postCount = await context.Posts.CountAsync(cancellationToken).ConfigureAwait(false);
        var hash = RuntimeHelpers.GetHashCode(context);
        TutorialConsole.WriteObservation(
            $"{label}: hash {hash}, posts lidos {postCount}. O hash identifica a instância CLR do DbContext neste processo.");
        return hash;
    }
}
