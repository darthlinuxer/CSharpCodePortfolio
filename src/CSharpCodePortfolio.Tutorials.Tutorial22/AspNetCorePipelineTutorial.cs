using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CSharpCodePortfolio.Tutorials.Tutorial22;

[Tutorial("22", "aspnet-core-pipeline", "ASP.NET Core com Pipeline Minimal API")]
public sealed class AspNetCorePipelineTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("22", "ASP.NET Core com Pipeline Minimal API");
        TutorialConsole.WriteContext(
            ("Tema", "ASP.NET Core"),
            ("Conceito", "Configuração, DI, middleware, filtros, cliente HTTP e endpoint em um host mínimo"),
            ("Runtime", ".NET 10"),
            ("Slug", "aspnet-core-pipeline"));
        TutorialConsole.WriteQuestion("Como uma requisição atravessa configuração, DI, middleware, filtros e endpoint em ASP.NET Core?");
        TutorialConsole.WriteHypothesis(
            "O host carrega configuração e valida opções antes de aceitar requisições.",
            "O contêiner de DI entrega handlers, middlewares e clientes HTTP tipados.",
            "Middlewares e filtros controlam a ordem de execução e podem encerrar a requisição antes do endpoint.");
        TutorialConsole.WritePreparation(
            "`AspNetCorePipelineScenario` sobe um `WebApplication` em porta local dinâmica.",
            "`RequestTraceMiddleware` e `EndpointTraceFilter` registram a ordem real do pipeline.",
            "`VerseClient` usa `IHttpClientFactory` com handler local para evitar rede externa.");

        TutorialConsole.WriteExperiment(
            1,
            "Configuração e serviços",
            "O host combina configuração em memória, valida opções e registra serviços usados pela rota.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a configuração alimenta `PortfolioApiOptions` e os serviços entram no contêiner.",
            typeof(AspNetCorePipelineScenario),
            nameof(AspNetCorePipelineScenario.CreateBuilder));

        TutorialConsole.WriteExperiment(
            2,
            "Pipeline HTTP",
            "A requisição passa por middleware de classe, middleware inline, ramificação condicional e endpoint.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: `UseWhen` encerra a requisição com cache quando a query string pede esse caminho.",
            typeof(AspNetCorePipelineScenario),
            nameof(AspNetCorePipelineScenario.ConfigurePipeline));

        TutorialConsole.WriteExperiment(
            3,
            "Rota Minimal API",
            "A rota recebe dependências por DI, chama um handler de consulta e usa um cliente HTTP tipado.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o endpoint orquestra a consulta e retorna um resultado HTTP tipado.",
            typeof(AspNetCorePipelineScenario),
            nameof(AspNetCorePipelineScenario.GetEmployeeAsync));

        var report = await new AspNetCorePipelineScenario().RunAsync(cancellationToken);
        VerifyReport(report);

        TutorialConsole.WriteEvidence(
            "Configuração resolvida",
            ("Base local", report.BaseAddress),
            ("URL da API", report.ApiUrl),
            ("Chave", report.ApiKey),
            ("Tentativas", report.RetryCount.ToString()));
        TutorialConsole.WriteEvidence(
            "Resposta do endpoint",
            ("Status", $"{(int)report.EmployeeRequest.StatusCode} {report.EmployeeRequest.StatusCode}"),
            ("Nome", report.EmployeeRequest.Employee?.Name ?? "sem resposta"),
            ("Referência", report.EmployeeRequest.Employee?.VerseReference ?? "sem resposta"));
        TutorialConsole.WriteEvidence(
            "Ordem observada",
            report.EmployeeRequest.Trace.Select((item, index) => ($"{index + 1:00}", item)).ToArray());
        TutorialConsole.WriteEvidence(
            "Ramificação com cache",
            ("Status", $"{(int)report.CachedRequest.StatusCode} {report.CachedRequest.StatusCode}"),
            ("Texto", report.CachedRequest.Employee?.VerseText ?? "sem resposta"),
            ("Filtro executado", report.CachedRequest.Trace.Contains("filter:endpoint:antes") ? "Sim" : "Não"));
        TutorialConsole.WriteEvidence(
            "Metadados documentáveis",
            ("Nome", report.EndpointName),
            ("Tags", string.Join(", ", report.EndpointTags)));

        TutorialConsole.WriteExperiment(
            4,
            "Teste automatizado",
            "O teste executa o host local e valida configuração, pipeline, endpoint, ramificação e cliente HTTP.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a verificação cobre a travessia completa da requisição.",
            "tests/CSharpCodePortfolio.Tutorials.Tutorial22.Tests/AspNetCorePipelineScenarioTests.cs");

        TutorialConsole.WriteObservation(
            "O suporte nativo de ASP.NET Core cobre host, configuração, DI, middleware, filtros de endpoint, `ILogger<T>` e metadados documentáveis sem pacotes externos para este cenário.");
        TutorialConsole.WriteConclusion(
            "Uma Minimal API fica testável quando a infraestrutura é pequena e explícita: o host sobe localmente, a rota recebe dependências por DI, o pipeline registra sua ordem e a execução termina sem depender de serviços externos.",
            TutorialConclusionKind.Success);
    }

    private static void VerifyReport(AspNetCorePipelineReport report)
    {
        if (report.EmployeeRequest.StatusCode != System.Net.HttpStatusCode.OK ||
            report.EmployeeRequest.Employee?.Name != "Ada Lovelace" ||
            !report.EmployeeRequest.Trace.Contains("middleware:classe:antes") ||
            !report.EmployeeRequest.Trace.Contains("filter:endpoint:antes") ||
            report.CachedRequest.Trace.Contains("filter:endpoint:antes"))
        {
            throw new InvalidOperationException("O pipeline ASP.NET Core não executou a sequência esperada.");
        }
    }
}
