using System.Diagnostics;
using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial01;

[Tutorial("01", "async-await-task", "Async/Await e Tasks")]
public sealed class AsyncAwaitTaskTutorial : ITutorial
{
    private static readonly TimeSpan KettleTime = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan CupPreparationTime = TimeSpan.FromMilliseconds(120);

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("01", "Async/Await e Tasks");
        TutorialConsole.WriteContext(
            ("Origem", "AsyncAwaitTask"),
            ("Conceito", "Comparar fluxo bloqueante com fluxo async/await"),
            ("Runtime", ".NET 10"),
            ("Slug", "async-await-task"));
        TutorialConsole.WriteQuestion("Quando `await` melhora um fluxo em vez de apenas trocar a sintaxe?");
        TutorialConsole.WriteHypothesis(
            "Se uma operacao de I/O esta pendente, o programa pode iniciar essa Task e executar trabalho independente antes de aguardar o resultado.",
            "O tempo total do fluxo async tende a se aproximar da operacao mais longa, nao da soma de todas as esperas.");
        TutorialConsole.WritePreparation(
            "O exemplo antigo fazia cha com `Task.Delay`: primeiro bloqueando a espera da chaleira, depois usando `await`.",
            "Aqui os tempos sao curtos para a execucao do tutorial, mas a relacao e a mesma de chamadas HTTP, disco, banco ou mensageria.");

        TutorialConsole.WriteExperiment(
            1,
            "Fluxo sincrono bloqueante",
            "A chaleira termina antes de qualquer preparo das xicaras comecar.");
        TutorialConsole.WriteCodeSnippet(
            "O trabalho independente fica preso atras da espera.",
            "SyncProcesses.cs",
            """
            var water = BoilWater();
            PrepareCups();
            return $"Pour {water} in cups";
            """);

        var syncRun = BrewTeaSynchronously();
        TutorialConsole.WriteEvidence(
            "Sincrono",
            ("Passos", string.Join(" -> ", syncRun.Steps)),
            ("Tempo medido", Format(syncRun.Elapsed)),
            ("Resultado", syncRun.Result));
        TutorialConsole.WriteConclusion(
            "A thread fica ocupada esperando a chaleira; o preparo das xicaras so acontece depois.",
            TutorialConclusionKind.Warning);

        TutorialConsole.WriteExperiment(
            2,
            "Fluxo async/await",
            "A Task da chaleira comeca, as xicaras sao preparadas, e so entao o codigo aguarda a agua.");
        TutorialConsole.WriteCodeSnippet(
            "O trabalho independente acontece antes do await final.",
            "AsyncProcesses.cs",
            """
            var boilingWater = BoilWaterAsync(cancellationToken);
            await PrepareCupsAsync(cancellationToken);
            var water = await boilingWater;
            return $"Pour {water} in cups";
            """);

        var asyncRun = await BrewTeaAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteEvidence(
            "Async",
            ("Passos", string.Join(" -> ", asyncRun.Steps)),
            ("Tempo medido", Format(asyncRun.Elapsed)),
            ("Resultado", asyncRun.Result),
            ("Ganho observado", Format(syncRun.Elapsed - asyncRun.Elapsed)));
        TutorialConsole.WriteObservation(
            "`await` nao cria velocidade magica: ele deixa o codigo continuar quando existe outro trabalho util enquanto a Task ainda nao terminou.");
        TutorialConsole.WriteConclusion(
            "Use async/await em operacoes naturalmente assincronas e propague o CancellationToken; nao use para CPU pesada sem uma razao explicita.",
            TutorialConclusionKind.Success);
    }

    private static TeaRun BrewTeaSynchronously()
    {
        var stopwatch = Stopwatch.StartNew();
        var steps = new List<string> { "liga chaleira e bloqueia" };

        Thread.Sleep(KettleTime);
        steps.Add("agua pronta");

        Thread.Sleep(CupPreparationTime);
        steps.Add("xicaras prontas");

        stopwatch.Stop();
        return new TeaRun("Pour Hot Water in cups", stopwatch.Elapsed, steps);
    }

    private static async Task<TeaRun> BrewTeaAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var steps = new List<string> { "liga chaleira" };

        var boilingWater = BoilWaterAsync(cancellationToken);
        steps.Add("prepara xicaras enquanto a chaleira trabalha");

        await Task.Delay(CupPreparationTime, cancellationToken).ConfigureAwait(false);
        steps.Add("xicaras prontas");

        var water = await boilingWater.ConfigureAwait(false);
        steps.Add("agua pronta");

        stopwatch.Stop();
        return new TeaRun($"Pour {water} in cups", stopwatch.Elapsed, steps);
    }

    private static async Task<string> BoilWaterAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(KettleTime, cancellationToken).ConfigureAwait(false);
        return "Hot Water";
    }

    private static string Format(TimeSpan elapsed)
    {
        return $"{elapsed.TotalMilliseconds:N0} ms";
    }

    private sealed record TeaRun(string Result, TimeSpan Elapsed, IReadOnlyList<string> Steps);
}
