using System.Net;
using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial20;

[Tutorial("20", "mock-http-client", "HttpClient com Handler Simulado")]
public sealed class MockHttpClientTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("20", "HttpClient com Handler Simulado");
        TutorialConsole.WriteContext(
            ("Tema", "Testes de clientes HTTP"),
            ("Conceito", "Controlar a resposta do `HttpClient` sem abrir conexão de rede"),
            ("Runtime", ".NET 10"),
            ("Slug", "mock-http-client"));
        TutorialConsole.WriteQuestion("Como testar um cliente HTTP sem depender de uma API em execução?");
        TutorialConsole.WriteHypothesis(
            "`HttpClient` delega o envio da requisição para um `HttpMessageHandler`.",
            "Um handler simulado devolve uma resposta previsível e registra a requisição recebida.",
            "O teste valida método, URI, status e conteúdo sem usar rede.");
        TutorialConsole.WritePreparation(
            "`PingClient` recebe um `HttpClient` por construtor.",
            "`StubHttpMessageHandler` responde com `200 OK` e conteúdo `true`.",
            "O mesmo handler registra a última requisição para validação.");

        TutorialConsole.WriteExperiment(
            1,
            "Cliente com dependência injetada",
            "O cliente usa o `HttpClient` recebido e não cria conexões por conta própria.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a chamada HTTP fica atrás de uma dependência injetada.",
            typeof(PingClient));

        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "true");
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://portfolio.local/")
        };

        var client = new PingClient(httpClient);
        var result = await client.CheckAsync(cancellationToken);

        VerifyResult(handler, result);

        TutorialConsole.WriteEvidence(
            "Resposta simulada",
            ("Status", $"{(int)result.StatusCode} {result.StatusCode}"),
            ("Conteúdo", result.Body),
            ("Sucesso", result.IsSuccess.ToString()));

        TutorialConsole.WriteExperiment(
            2,
            "Handler controlado",
            "O handler substitui a rede, registra a requisição e entrega uma resposta conhecida.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: `SendAsync` é o ponto de controle do `HttpClient`.",
            typeof(StubHttpMessageHandler));

        var request = handler.LastRequest ?? throw new InvalidOperationException("A requisição não foi registrada.");
        TutorialConsole.WriteEvidence(
            "Requisição observada",
            ("Método", request.Method.Method),
            ("URI", request.Uri.ToString()),
            ("Chamadas", handler.CallCount.ToString()));

        TutorialConsole.WriteExperiment(
            3,
            "Teste automatizado",
            "O teste usa o mesmo handler para validar resultado e requisição enviada.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o teste prova o comportamento sem iniciar servidor HTTP.",
            "tests/CSharpCodePortfolio.Tutorials.Tutorial20.Tests/PingClientTests.cs");

        TutorialConsole.WriteObservation(
            "O teste fica rápido porque valida o contrato do cliente e substitui apenas a fronteira de rede.");
        TutorialConsole.WriteConclusion(
            "Para testar código com `HttpClient`, controle o `HttpMessageHandler`: ele é o ponto mínimo que simula a resposta e preserva a chamada real do cliente.",
            TutorialConclusionKind.Success);
    }

    private static void VerifyResult(StubHttpMessageHandler handler, PingResult result)
    {
        var request = handler.LastRequest;

        if (result.StatusCode != HttpStatusCode.OK ||
            result.Body != "true" ||
            !result.IsSuccess ||
            handler.CallCount != 1 ||
            request is null ||
            request.Method != HttpMethod.Get ||
            request.Uri.AbsolutePath != "/ping")
        {
            throw new InvalidOperationException("O handler simulado não registrou a chamada HTTP esperada.");
        }
    }
}
