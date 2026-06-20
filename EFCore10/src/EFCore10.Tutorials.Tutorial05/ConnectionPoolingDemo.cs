using System.Data;
using System.Runtime.CompilerServices;
using EFCore10.Shared;
using EFCore10.Tutorials.Tutorial05.Context;
using EFCore10.Tutorials.Tutorial05.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial05;

internal sealed class ConnectionPoolingDemo(IDbContextFactory<PoolingContext> dbContextFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteQuestion(
            "DbContext pooling e connection pooling reutilizam a mesma coisa?");
        TutorialConsole.WriteHypothesis(
            "DbContext pooling reutiliza objetos DbContext.",
            "Connection pooling é outra camada, controlada pelo provedor ADO.NET.");

        await ResetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await SeedBlogAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WritePreparation(
            "O schema do banco foi recriado.",
            "Foi inserido um blog para que as consultas executem trabalho real.");

        await DemonstrateContextPoolingAsync(cancellationToken).ConfigureAwait(false);
        await DemonstrateConnectionBoundaryAsync(cancellationToken).ConfigureAwait(false);
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

        context.Blogs.Add(new Blog { Url = "https://learn.microsoft.com/ef/core" });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task DemonstrateContextPoolingAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteExperiment(
            1,
            "DbContext pooling reutiliza o objeto de contexto?",
            "Criar, consultar e descartar duas instâncias de DbContext usando uma factory pooled com pool size 1.");

        var firstHash = await CreateAndReadContextAsync("Primeiro DbContext", cancellationToken).ConfigureAwait(false);
        var secondHash = await CreateAndReadContextAsync("Segundo DbContext", cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteConclusion(
            firstHash == secondHash
                ? "O hash se repetiu; isso mostra reutilização do objeto DbContext pelo pool do EF Core."
                : "O hash mudou; o pool pode entregar outra instância, mas esse teste ainda trata apenas do objeto DbContext.",
            firstHash == secondHash ? TutorialConclusionKind.Success : TutorialConclusionKind.Warning);
    }

    private async Task DemonstrateConnectionBoundaryAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteExperiment(
            2,
            "DbContext pooling mantém a conexão ADO.NET aberta?",
            "Observar o estado da conexão ADO.NET antes e depois de uma query EF e de uma abertura explícita.");

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var connection = context.Database.GetDbConnection();

        TutorialConsole.WriteObservation(
            $"DbContext em uso: hash {RuntimeHelpers.GetHashCode(context)}. Estado da conexão ADO.NET antes da query EF: {connection.State}.");

        var blogCount = await context.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteObservation(
            $"A query leu {blogCount} blog(s). Depois da query EF, o estado da conexão ADO.NET é {connection.State}.");

        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteObservation(
            $"Depois de OpenConnectionAsync explícito, o estado da conexão ADO.NET é {connection.State}.");

        await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        TutorialConsole.WriteObservation(
            $"Depois de CloseConnectionAsync explícito, o estado da conexão ADO.NET é {connection.State}.");

        TutorialConsole.WriteConclusion(
            connection.State == ConnectionState.Closed
                ? "DbContext pooling reutiliza objetos de contexto; connection pooling é responsabilidade do provedor ADO.NET."
                : "Resultado inesperado: a conexão ADO.NET permaneceu aberta.",
            connection.State == ConnectionState.Closed ? TutorialConclusionKind.Success : TutorialConclusionKind.Failure);
    }

    private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteCleanup("O banco SQLite de demonstração foi removido.");
    }

    private async Task<int> CreateAndReadContextAsync(
        string label,
        CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var blogCount = await context.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
        var hash = RuntimeHelpers.GetHashCode(context);
        TutorialConsole.WriteObservation(
            $"{label}: hash {hash}, blogs lidos {blogCount}. O hash identifica a instância CLR do DbContext neste processo.");
        return hash;
    }
}
