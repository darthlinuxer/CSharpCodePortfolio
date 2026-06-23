using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial21;

[Tutorial("21", "masstransit-inmemory", "MassTransit com Transporte em Memória")]
public sealed class MassTransitInMemoryTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("21", "MassTransit com Transporte em Memória");
        TutorialConsole.WriteContext(
            ("Tema", "Mensageria assíncrona"),
            ("Conceito", "Publicar e consumir uma mensagem com MassTransit sem broker externo"),
            ("Runtime", ".NET 10"),
            ("Slug", "masstransit-inmemory"));
        TutorialConsole.WriteQuestion("Como validar o fluxo de publicação e consumo sem depender de RabbitMQ ou outro serviço?");
        TutorialConsole.WriteHypothesis(
            "O transporte em memória executa o pipeline real do MassTransit dentro do processo.",
            "Um endpoint de recebimento registra o handler da mensagem.",
            "A publicação entrega a mensagem ao endpoint e permite validar a evidência de consumo.");
        TutorialConsole.WritePreparation(
            "`PortfolioMessage` representa o contrato trafegado no bus.",
            "`MassTransitInMemoryScenario` inicia o bus, publica uma mensagem e aguarda o consumo.",
            "`ReceivedMessageProbe` registra a primeira mensagem recebida e evita loop infinito.");

        TutorialConsole.WriteExperiment(
            1,
            "Configuração do bus",
            "Cria um bus MassTransit em memória com um endpoint chamado `portfolio-messages`.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o endpoint em memória registra o handler para `PortfolioMessage`.",
            typeof(MassTransitInMemoryScenario),
            nameof(MassTransitInMemoryScenario.RunAsync),
            new CodeExcerpt(3, 17, "Bus e endpoint"));

        TutorialConsole.WriteExperiment(
            2,
            "Publicação e consumo",
            "Publica uma mensagem, aguarda o handler recebê-la e encerra o bus.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a execução é finita e sempre para o bus no `finally`.",
            typeof(MassTransitInMemoryScenario),
            nameof(MassTransitInMemoryScenario.RunAsync),
            new CodeExcerpt(18, 35, "Publicação, espera e parada"));

        var report = await new MassTransitInMemoryScenario().RunAsync(cancellationToken);
        VerifyReport(report);

        TutorialConsole.WriteEvidence(
            "Mensagem publicada",
            ("Fila", report.QueueName),
            ("Id", report.Published.MessageId.ToString()),
            ("Texto", report.Published.Text));
        TutorialConsole.WriteEvidence(
            "Mensagem consumida",
            ("Id", report.Consumed.MessageId.ToString()),
            ("Texto", report.Consumed.Text),
            ("Total consumido", report.ConsumedMessages.ToString()));

        TutorialConsole.WriteExperiment(
            3,
            "Teste automatizado",
            "O teste executa o cenário completo e compara a mensagem publicada com a consumida.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o teste prova que o pipeline entregou uma mensagem.",
            "tests/CSharpCodePortfolio.Tutorials.Tutorial21.Tests/MassTransitInMemoryScenarioTests.cs");

        TutorialConsole.WriteObservation(
            "O transporte em memória preserva a semântica de publicação e consumo sem exigir configuração de infraestrutura.");
        TutorialConsole.WriteConclusion(
            "MassTransit pode ser demonstrado e testado localmente com transporte em memória: o bus inicia, publica, entrega ao endpoint e para de forma determinística.",
            TutorialConclusionKind.Success);
    }

    private static void VerifyReport(MassTransitDeliveryReport report)
    {
        if (report.ConsumedMessages != 1 ||
            report.Published.MessageId != report.Consumed.MessageId ||
            report.Published.Text != report.Consumed.Text)
        {
            throw new InvalidOperationException("A mensagem consumida deve corresponder à mensagem publicada.");
        }
    }
}
