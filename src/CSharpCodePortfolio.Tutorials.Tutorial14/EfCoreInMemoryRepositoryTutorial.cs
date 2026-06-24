using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial14;

[Tutorial("14", "efcore-inmemory-repository", "IRepository com EF Core InMemory")]
public sealed class EfCoreInMemoryRepositoryTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("14", "IRepository com EF Core InMemory");
        TutorialConsole.WriteContext(
            ("Tema", "Repository Pattern com EF Core"),
            ("Conceito", "O repositório encapsula DbSet e a unidade de trabalho centraliza SaveChanges"),
            ("Provider", "Microsoft.EntityFrameworkCore.InMemory"),
            ("Runtime", ".NET 10"),
            ("Slug", "efcore-inmemory-repository"));
        TutorialConsole.WriteQuestion("Como combinar `IRepository`, Unit of Work e EF Core InMemory em um fluxo CRUD completo?");
        TutorialConsole.WriteHypothesis(
            "`IRepository` expõe operações de leitura e escrita sem vazar `DbSet` para o fluxo de uso.",
            "`EfRepository` traduz esse contrato para operações do EF Core.",
            "`ClientUnitOfWork` executa `SaveChangesAsync` em um ponto explícito de commit.");
        TutorialConsole.WritePreparation(
            "O banco em memória recebe um nome único para cada execução.",
            "O catálogo de clientes usa a unidade de trabalho, não o `DbContext` diretamente.",
            "O cenário cadastra, consulta, atualiza e remove clientes.");

        TutorialConsole.WriteExperiment(
            1,
            "Contrato do repositório",
            "Define o conjunto de operações que o fluxo de uso precisa para trabalhar com clientes.");
        TutorialConsole.WriteCodeSnippet(
            "O contrato aceita predicados para consultas e deixa o commit fora do repositório.",
            "src/CSharpCodePortfolio.Tutorials.Tutorial14/IRepository.cs");

        TutorialConsole.WriteExperiment(
            2,
            "Implementação EF Core",
            "Usa `DbSet<T>` para executar operações genéricas e mantém `SaveChangesAsync` na unidade de trabalho.");
        TutorialConsole.WriteCodeSnippet(
            "Leituras usam `AsNoTracking`; comandos ficam pendentes até o commit.",
            "src/CSharpCodePortfolio.Tutorials.Tutorial14/EfRepository.cs");

        TutorialConsole.WriteExperiment(
            3,
            "Fluxo CRUD completo",
            "Executa inclusão, listagem, atualização, consulta por predicado e remoção.");
        TutorialConsole.WriteCodeSnippet(
            "A unidade de trabalho concentra o commit das alterações feitas pelo catálogo.",
            typeof(EfCoreInMemoryRepositoryTutorial),
            nameof(RunScenarioAsync),
            new CodeExcerpt(5, 19, "Fluxo CRUD pelo catálogo"));

        var databaseName = $"dbClients-repository-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<ClientDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        await using var context = new ClientDbContext(options);
        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var unitOfWork = new ClientUnitOfWork(context);
        var catalog = new ClientCatalog(unitOfWork);
        var report = await RunScenarioAsync(catalog, cancellationToken);

        VerifyReport(report);

        TutorialConsole.WriteEvidence(
            "Repository + Unit of Work",
            ("Após carga inicial", FormatClients(report.AfterSeed)),
            ("Após inclusão", FormatClients(report.AfterInsert)),
            ("Cliente atualizado", report.UpdatedClient is null ? "não encontrado" : FormatClient(report.UpdatedClient)),
            ("Consulta por nome", FormatClients(report.SearchResult)),
            ("Remoção", report.Removed ? "cliente removido" : "cliente não encontrado"),
            ("Estado final", FormatClients(report.FinalClients)));

        TutorialConsole.WriteObservation(
            "O `DbContext` fica restrito ao repositório e à unidade de trabalho; o catálogo expressa o caso de uso.");
        TutorialConsole.WriteConclusion(
            "`IRepository` com EF Core InMemory é suficiente para ensinar CRUD, predicados e commit explícito sem depender de um banco externo.",
            TutorialConclusionKind.Success);
    }

    private static async Task<ClientReport> RunScenarioAsync(
        ClientCatalog catalog,
        CancellationToken cancellationToken)
    {
        await catalog.RegisterAsync(
            new Client { Id = 100, Name = "John Rambo", Email = "rambo@example.com" },
            cancellationToken);
        var afterSeed = await catalog.ListClientsAsync(cancellationToken);

        await catalog.RegisterAsync(
            new Client { Id = 1, Name = "Camilo", Email = "camilo@example.com" },
            cancellationToken);
        var afterInsert = await catalog.ListClientsAsync(cancellationToken);

        await catalog.ChangeNameAsync(1, "Camilo Chaves", cancellationToken);
        var updatedClient = await catalog.GetClientAsync(1, cancellationToken);
        var searchResult = await catalog.FindClientsByNameAsync("Camilo", cancellationToken);
        var removed = await catalog.RemoveAsync(100, cancellationToken);
        var finalClients = await catalog.ListClientsAsync(cancellationToken);

        return new ClientReport(afterSeed, afterInsert, updatedClient, searchResult, removed, finalClients);
    }

    private static void VerifyReport(ClientReport report)
    {
        if (report.AfterSeed.Count != 1 || report.AfterSeed[0].Id != 100)
        {
            throw new InvalidOperationException("A carga inicial deve conter apenas o cliente John Rambo.");
        }

        if (report.AfterInsert.Count != 2)
        {
            throw new InvalidOperationException("A inclusão deve manter dois clientes no banco em memória.");
        }

        if (report.UpdatedClient is not { Id: 1, Name: "Camilo Chaves" })
        {
            throw new InvalidOperationException("A atualização deve alterar o nome do cliente Camilo.");
        }

        if (report.SearchResult.Count != 1 || report.SearchResult[0].Id != 1)
        {
            throw new InvalidOperationException("A consulta por predicado deve localizar o cliente atualizado.");
        }

        if (!report.Removed)
        {
            throw new InvalidOperationException("A remoção do cliente inicial deve retornar sucesso.");
        }

        if (report.FinalClients.Count != 1 || report.FinalClients[0].Id != 1)
        {
            throw new InvalidOperationException("O estado final deve manter apenas o cliente atualizado.");
        }
    }

    private static string FormatClients(IEnumerable<Client> clients)
    {
        var formattedClients = clients.Select(FormatClient).ToArray();
        return formattedClients.Length == 0 ? "nenhum cliente" : string.Join(" | ", formattedClients);
    }

    private static string FormatClient(Client client) =>
        $"#{client.Id} {client.Name} <{client.Email}>";
}
