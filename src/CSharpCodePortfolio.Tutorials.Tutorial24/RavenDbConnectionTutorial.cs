using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial24;

[Tutorial("24", "raven-db-connection", "RavenDB com DocumentStore")]
public sealed class RavenDbConnectionTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("24", "RavenDB com DocumentStore");
        TutorialConsole.WriteContext(
            ("Tema", "RavenDB"),
            ("Conceito", "Configuração, seleção de URL, DocumentStore e fluxo de sessão"),
            ("Runtime", ".NET 10"),
            ("Slug", "raven-db-connection"));
        TutorialConsole.WriteQuestion("Como preparar uma conexão RavenDB sem espalhar configuração e sessões pelo código?");
        TutorialConsole.WriteHypothesis(
            "A configuração separa URLs do host e de container para evitar decisões espalhadas.",
            "O `DocumentStore` concentra database e convenções de sessão.",
            "O CRUD fica previsível quando cada operação abre sessão, executa o comando e confirma com `SaveChanges`.");
        TutorialConsole.WritePreparation(
            "`RavenConnectionScenario` usa a mesma configuração do tutorial, mas não exige servidor RavenDB ativo.",
            "O `DocumentStore` é criado com tipos reais do RavenDB.Client e não chama `Initialize` durante o teste.",
            "O fluxo de sessão mostra a ordem das operações que uma API usaria para gravar, consultar e carregar usuários.");

        TutorialConsole.WriteExperiment(
            1,
            "Configuração por ambiente",
            "Seleciona URLs diferentes quando o código roda no host ou dentro de container.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a seleção de URLs fica em um ponto único.",
            typeof(RavenDocumentStoreFactory),
            nameof(RavenDocumentStoreFactory.SelectUrls));

        TutorialConsole.WriteExperiment(
            2,
            "DocumentStore",
            "Cria o store com database, URLs e convenções de sessão antes da conexão com o servidor.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o store recebe URLs, database e convenções de sessão.",
            typeof(RavenDocumentStoreFactory),
            nameof(RavenDocumentStoreFactory.CreateConfiguredStore));

        TutorialConsole.WriteExperiment(
            3,
            "Fluxo de sessão",
            "Representa o caminho usado por endpoints de criação, consulta paginada e carregamento por id.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o roteiro mostra a ordem esperada de uma sessão RavenDB.",
            typeof(RavenSessionFlow),
            nameof(RavenSessionFlow.DescribeUserFlow));

        var report = RavenConnectionScenario.Run();

        TutorialConsole.WriteEvidence(
            "Store configurado",
            ("Database", report.Database),
            ("URL no host", string.Join(" | ", report.HostUrls)),
            ("URL em container", string.Join(" | ", report.ContainerUrls)),
            ("Requisições por sessão", report.MaxRequestsPerSession.ToString()),
            ("Concorrência otimista", report.UseOptimisticConcurrency ? "Sim" : "Não"));
        TutorialConsole.WriteEvidence(
            "Sessão planejada",
            report.SessionSteps.Select((step, index) => ($"{index + 1:00}", step)).ToArray());

        TutorialConsole.WriteExperiment(
            4,
            "Teste automatizado",
            "O teste valida configuração, convenções do store e roteiro de sessão sem depender de RavenDB externo.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a verificação cobre o contrato local do tutorial.",
            "tests/CSharpCodePortfolio.Tutorials.Tutorial24.Tests/RavenConnectionScenarioTests.cs");

        TutorialConsole.WriteObservation(
            "A criação real da database e as operações de rede pertencem ao ambiente que tem RavenDB disponível; o tutorial valida a parte que deve ser determinística no código.");
        TutorialConsole.WriteConclusion(
            "A conexão RavenDB fica controlável quando configuração, `DocumentStore` e fluxo de sessão têm responsabilidades pequenas e verificáveis.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }
}
