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
            ("Tema", "Async/await com Task"),
            ("Conceito", "Comparar fluxo bloqueante com fluxo async/await"),
            ("Runtime", ".NET 10"),
            ("Slug", "async-await-task"));
        TutorialConsole.WriteQuestion("Quando `await` melhora um fluxo em vez de apenas trocar a sintaxe?");
        TutorialConsole.WriteHypothesis(
            "Se uma operação de entrada e saída está pendente, o programa pode iniciar essa Task e executar trabalho independente antes de aguardar o resultado.",
            "O tempo total do fluxo assíncrono tende a se aproximar da operação mais longa, não da soma de todas as esperas.");
        TutorialConsole.WritePreparation(
            "O cenário prepara chá com `Task.Delay`: primeiro bloqueando a espera da chaleira, depois usando `await`.",
            "Os blocos de código exibidos abaixo são lidos dos métodos reais desta classe de tutorial.",
            "Os tempos são curtos para a execução do tutorial, mas a relação é a mesma de chamadas HTTP, disco, banco de dados ou mensageria.");

        TutorialConsole.WriteExperiment(
            1,
            "Fluxo síncrono bloqueante",
            "A chaleira termina antes de qualquer preparo das xícaras começar.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o trabalho independente fica preso atrás da espera.",
            typeof(AsyncAwaitTaskTutorial),
            nameof(BrewTeaSynchronously));

        var syncRun = BrewTeaSynchronously();
        TutorialConsole.WriteEvidence(
            "Síncrono",
            ("Passos", string.Join(" -> ", syncRun.Steps)),
            ("Tempo medido", Format(syncRun.Elapsed)),
            ("Resultado", syncRun.Result));
        TutorialConsole.WriteConclusion(
            "A thread fica ocupada esperando a chaleira; o preparo das xícaras só acontece depois.",
            TutorialConclusionKind.Warning);

        TutorialConsole.WriteExperiment(
            2,
            "Fluxo async/await",
            "A Task da chaleira começa, as xícaras são preparadas, e só então o código aguarda a água.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o trabalho independente acontece antes do await final.",
            typeof(AsyncAwaitTaskTutorial),
            nameof(BrewTeaAsync));

        var asyncRun = await BrewTeaAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteEvidence(
            "Async",
            ("Passos", string.Join(" -> ", asyncRun.Steps)),
            ("Tempo medido", Format(asyncRun.Elapsed)),
            ("Resultado", asyncRun.Result),
            ("Ganho observado", Format(syncRun.Elapsed - asyncRun.Elapsed)));
        TutorialConsole.WriteObservation(
            "`await` não cria velocidade mágica: ele deixa o código continuar quando existe outro trabalho útil enquanto a Task ainda não terminou.");
        TutorialConsole.WriteConclusion(
            "Use async/await em operações naturalmente assíncronas e propague o CancellationToken; não use para CPU pesada sem uma razão explícita.",
            TutorialConclusionKind.Success);
    }

    private static TeaRun BrewTeaSynchronously()
    {
        var stopwatch = Stopwatch.StartNew();
        var steps = new List<string> { "liga a chaleira e bloqueia" };

        Thread.Sleep(KettleTime);
        steps.Add("água pronta");

        Thread.Sleep(CupPreparationTime);
        steps.Add("xícaras prontas");

        stopwatch.Stop();
        return new TeaRun("Pour Hot Water in cups", stopwatch.Elapsed, steps);
    }

    private static async Task<TeaRun> BrewTeaAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var steps = new List<string> { "liga a chaleira" };

        var boilingWater = BoilWaterAsync(cancellationToken);
        steps.Add("prepara xícaras enquanto a chaleira trabalha");

        await Task.Delay(CupPreparationTime, cancellationToken).ConfigureAwait(false);
        steps.Add("xícaras prontas");

        var water = await boilingWater.ConfigureAwait(false);
        steps.Add("água pronta");

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
