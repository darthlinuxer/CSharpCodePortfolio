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
        TutorialConsole.WriteCodeSnippet(
            "Criar e descartar o DbContext via factory pooled.",
            "ConnectionPoolingDemo.cs",
            """
            await using var context =
                await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var blogCount = await context.Blogs.CountAsync(cancellationToken);
            var hash = RuntimeHelpers.GetHashCode(context);
            """);

        var firstContext = await CreateAndReadContextAsync(cancellationToken).ConfigureAwait(false);
        var secondContext = await CreateAndReadContextAsync(cancellationToken).ConfigureAwait(false);
        var sameInstance = firstContext.Hash == secondContext.Hash;

        TutorialConsole.WriteEvidence(
            "DbContext pooling",
            ("Pool size", ConnectionPoolingTutorial.DemoPoolSize.ToString()),
            ("Hash primeiro DbContext", firstContext.Hash.ToString()),
            ("Blogs lidos no primeiro", firstContext.BlogCount.ToString()),
            ("Hash segundo DbContext", secondContext.Hash.ToString()),
            ("Blogs lidos no segundo", secondContext.BlogCount.ToString()),
            ("Mesmo objeto CLR?", FormatBoolean(sameInstance)));

        TutorialConsole.WriteConclusion(
            sameInstance
                ? "O hash se repetiu; isso mostra reutilização do objeto DbContext pelo pool do EF Core."
                : "O hash mudou; o pool pode entregar outra instância, mas esse teste ainda trata apenas do objeto DbContext.",
            sameInstance ? TutorialConclusionKind.Success : TutorialConclusionKind.Warning);
    }

    private async Task DemonstrateConnectionBoundaryAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteExperiment(
            2,
            "DbContext pooling mantém a conexão ADO.NET aberta?",
            "Observar o estado da conexão ADO.NET antes e depois de uma query EF e de uma abertura explícita.");
        TutorialConsole.WriteCodeSnippet(
            "Obter a conexão ADO.NET por baixo do DbContext.",
            "ConnectionPoolingDemo.cs",
            """
            await using var context =
                await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var connection = context.Database.GetDbConnection();
            var beforeQuery = connection.State;

            var blogCount = await context.Blogs.CountAsync(cancellationToken);
            var afterQuery = connection.State;
            """);
        TutorialConsole.WriteCodeSnippet(
            "Abrir e fechar a conexão explicitamente quando esse controle for necessário.",
            "ConnectionPoolingDemo.cs",
            """
            await context.Database.OpenConnectionAsync(cancellationToken);
            var openState = connection.State;

            await context.Database.CloseConnectionAsync();
            var closedState = connection.State;
            """);

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var connection = context.Database.GetDbConnection();
        var contextHash = RuntimeHelpers.GetHashCode(context);
        var stateBeforeQuery = connection.State;

        var blogCount = await context.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
        var stateAfterEfQuery = connection.State;

        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var stateAfterExplicitOpen = connection.State;

        await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        var stateAfterExplicitClose = connection.State;

        TutorialConsole.WriteEvidence(
            "Linha do tempo da conexão ADO.NET",
            ("Hash do DbContext", contextHash.ToString()),
            ("Estado antes da query EF", stateBeforeQuery.ToString()),
            ("Blogs lidos pela query", blogCount.ToString()),
            ("Estado depois da query EF", stateAfterEfQuery.ToString()),
            ("Estado depois de OpenConnectionAsync", stateAfterExplicitOpen.ToString()),
            ("Estado depois de CloseConnectionAsync", stateAfterExplicitClose.ToString()),
            ("Connection pooling controlado por", "provedor ADO.NET"));

        TutorialConsole.WriteConclusion(
            stateAfterExplicitClose == ConnectionState.Closed
                ? "DbContext pooling reutiliza objetos de contexto; connection pooling é responsabilidade do provedor ADO.NET."
                : "Resultado inesperado: a conexão ADO.NET permaneceu aberta.",
            stateAfterExplicitClose == ConnectionState.Closed ? TutorialConclusionKind.Success : TutorialConclusionKind.Failure);
    }

    private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteCleanup("O banco SQLite de demonstração foi removido.");
    }

    private async Task<(int Hash, int BlogCount)> CreateAndReadContextAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var blogCount = await context.Blogs.CountAsync(cancellationToken).ConfigureAwait(false);
        var hash = RuntimeHelpers.GetHashCode(context);
        return (hash, blogCount);
    }

    private static string FormatBoolean(bool value)
    {
        return value ? "sim" : "não";
    }
}
