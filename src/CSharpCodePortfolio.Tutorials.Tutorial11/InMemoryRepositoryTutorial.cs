using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial11;

[Tutorial("11", "in-memory-repository", "IRepository em memória sem Entity Framework")]
public sealed class InMemoryRepositoryTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("11", "IRepository em memória sem Entity Framework");
        TutorialConsole.WriteContext(
            ("Tema", "Repository Pattern"),
            ("Conceito", "Um contrato de repositório permite trocar armazenamento sem mudar o fluxo de uso"),
            ("Armazenamento", "Dictionary<TKey, T> em memória"),
            ("Runtime", ".NET 10"),
            ("Slug", "in-memory-repository"));
        TutorialConsole.WriteQuestion("Como implementar um `IRepository` completo em memória sem usar Entity Framework?");
        TutorialConsole.WriteHypothesis(
            "O contrato expõe operações de leitura e escrita por chave.",
            "A implementação em memória guarda os objetos em um dicionário.",
            "O código de uso depende do contrato, não do mecanismo de armazenamento.");
        TutorialConsole.WritePreparation(
            "O exemplo cadastra dois clientes.",
            "Depois consulta, atualiza e remove registros.",
            "A checagem final confirma que o repositório manteve o estado esperado.");

        IRepository<Client, int> repository = new InMemoryRepository<Client, int>(static client => client.Id);
        var catalog = new ClientCatalog(repository);

        TutorialConsole.WriteExperiment(
            1,
            "Contrato mínimo",
            "Define operações de lista, busca por chave, inclusão, atualização e remoção.");
        TutorialConsole.WriteCodeSnippet(
            "O contrato não conhece EF, banco de dados nem infraestrutura externa.",
            "src/CSharpCodePortfolio.Tutorials.Tutorial11/IRepository.cs");

        TutorialConsole.WriteExperiment(
            2,
            "Execução em memória",
            "Usa o contrato para manipular clientes sem acoplar o fluxo de uso ao dicionário interno.");
        TutorialConsole.WriteCodeSnippet(
            "A chave é extraída por uma função, mantendo o repositório genérico.",
            "src/CSharpCodePortfolio.Tutorials.Tutorial11/InMemoryRepository.cs");
        TutorialConsole.WriteCodeSnippet(
            "O catálogo usa o contrato e não conhece o dicionário interno.",
            typeof(ClientCatalog),
            nameof(ClientCatalog.Register),
            nameof(ClientCatalog.ChangeEmail),
            nameof(ClientCatalog.Remove));

        catalog.Register(new Client(1, "Camilo", "camilo@example.com"));
        catalog.Register(new Client(2, "Aline", "aline@example.com"));
        var afterInsert = catalog.ListClients();
        var foundClient = catalog.GetClient(2);
        var updated = catalog.ChangeEmail(2, "aline.dev@example.com");
        var removed = catalog.Remove(1);
        var finalClients = catalog.ListClients();

        VerifyRepositoryBehavior(afterInsert, foundClient, updated, removed, finalClients);

        TutorialConsole.WriteEvidence(
            "Operações executadas",
            ("Após inclusão", FormatClients(afterInsert)),
            ("Cliente consultado", foundClient is null ? "não encontrado" : FormatClient(foundClient)),
            ("Atualização", updated ? "e-mail atualizado" : "cliente não encontrado"),
            ("Remoção", removed ? "cliente removido" : "cliente não encontrado"),
            ("Estado final", FormatClients(finalClients)));

        TutorialConsole.WriteObservation(
            "O catálogo usa apenas `IRepository<Client, int>`; a implementação pode mudar sem alterar o fluxo de cadastro.");
        TutorialConsole.WriteConclusion(
            "Um repositório em memória é útil para demonstrar regras de persistência, testes rápidos e protótipos sem carregar infraestrutura externa.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static void VerifyRepositoryBehavior(
        IReadOnlyList<Client> afterInsert,
        Client? foundClient,
        bool updated,
        bool removed,
        IReadOnlyList<Client> finalClients)
    {
        if (afterInsert.Count != 2)
        {
            throw new InvalidOperationException("A inclusão deve manter dois clientes no repositório.");
        }

        if (foundClient is not { Id: 2, Name: "Aline" })
        {
            throw new InvalidOperationException("A consulta por chave deve localizar a cliente Aline.");
        }

        if (!updated || !removed)
        {
            throw new InvalidOperationException("Atualização e remoção devem retornar sucesso.");
        }

        if (finalClients is not [{ Id: 2, Email: "aline.dev@example.com" }])
        {
            throw new InvalidOperationException("O estado final deve manter apenas a cliente atualizada.");
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
