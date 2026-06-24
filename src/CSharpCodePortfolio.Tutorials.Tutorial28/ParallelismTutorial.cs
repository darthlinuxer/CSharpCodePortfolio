using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial28;

[Tutorial("28", "parallelism", "Paralelismo com Tasks, PLINQ e Parallel")]
public sealed class ParallelismTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("28", "Paralelismo com Tasks, PLINQ e Parallel");
        TutorialConsole.WriteContext(
            ("Tema", "Paralelismo"),
            ("Conceito", "Parallel.Invoke, Task.WhenAll, Parallel.ForEachAsync, PLINQ, CPU bound e cancelamento"),
            ("Runtime", ".NET 10"),
            ("Slug", "parallelism"));
        TutorialConsole.WriteQuestion("Quando usar fan-out assíncrono, paralelismo limitado e PLINQ?");
        TutorialConsole.WriteHypothesis(
            "Task.WhenAll é direto quando todos os trabalhos assíncronos podem iniciar ao mesmo tempo.",
            "Parallel.ForEachAsync é melhor quando o processamento assíncrono precisa de limite de concorrência.",
            "PLINQ funciona bem para consultas puras em coleções já carregadas.",
            "Trabalho de CPU precisa controlar o grau de paralelismo e proteger estado compartilhado.");
        TutorialConsole.WritePreparation(
            "O tutorial usa atrasos pequenos para simular chamadas externas sem depender de rede.",
            "`ConcurrencyProbe` mede a maior quantidade de operações simultâneas observada.",
            "`CpuPrimeCounter` calcula primos com `Parallel.ForEach` e `ConcurrentBag`.");

        TutorialConsole.WriteExperiment(
            1,
            "Ações independentes",
            "Executa três ações síncronas independentes com `Parallel.Invoke`.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: ações independentes são disparadas sem compartilhar estado mutável.",
            typeof(ParallelismScenario),
            nameof(ParallelismScenario.RunIndependentActions));

        TutorialConsole.WriteExperiment(
            2,
            "Fan-out assíncrono",
            "Compara execução sequencial com `Task.WhenAll` para trabalhos assíncronos.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: `Task.WhenAll` cria as tarefas e aguarda todas terminarem.",
            typeof(ParallelismScenario),
            nameof(ParallelismScenario.RunWithWhenAllAsync));

        TutorialConsole.WriteExperiment(
            3,
            "Concorrência limitada",
            "Usa `Parallel.ForEachAsync` com `MaxDegreeOfParallelism` e `CancellationToken`.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o limite de concorrência fica no `ParallelOptions`.",
            typeof(ParallelismScenario),
            nameof(ParallelismScenario.RunWithForEachAsync));

        TutorialConsole.WriteExperiment(
            4,
            "PLINQ e CPU",
            "Separa pares e ímpares com PLINQ e conta primos com `Parallel.ForEach`.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: PLINQ expressa consulta paralela sem estado compartilhado.",
            typeof(ParallelismScenario),
            nameof(ParallelismScenario.PartitionWithPlinq));
        TutorialConsole.WriteCodeSnippet(
            "Código real: CPU bound usa grau máximo e coleção concorrente.",
            typeof(CpuPrimeCounter),
            nameof(CpuPrimeCounter.FindPrimesUntil));

        var report = await ParallelismScenario.RunAsync(cancellationToken);

        TutorialConsole.WriteEvidence(
            "Ações",
            ("Total", report.IndependentActions.Count.ToString()),
            ("Itens", string.Join(", ", report.IndependentActions)));
        TutorialConsole.WriteEvidence(
            "Execução assíncrona",
            ("Sequencial", $"{report.Sequential.ElapsedMilliseconds} ms | concorrência {report.Sequential.MaxConcurrency}"),
            ("Task.WhenAll", $"{report.FanOut.ElapsedMilliseconds} ms | concorrência {report.FanOut.MaxConcurrency}"),
            ("Parallel.ForEachAsync", $"{report.Limited.ElapsedMilliseconds} ms | concorrência {report.Limited.MaxConcurrency}"),
            ("Soma dos resultados", report.FanOut.TotalScore.ToString()));
        TutorialConsole.WriteEvidence(
            "PLINQ",
            ("Pares", string.Join(", ", report.Partition.EvenNumbers)),
            ("Ímpares", string.Join(", ", report.Partition.OddNumbers)));
        TutorialConsole.WriteEvidence(
            "CPU e cancelamento",
            ("Primos até 30", string.Join(", ", report.Primes)),
            ("Cancelamento observado", report.CancellationObserved ? "Sim" : "Não"));
        TutorialConsole.WriteEvidence(
            "Checklist",
            report.Checklist.Select((item, index) => ($"{index + 1:00}", item)).ToArray());

        TutorialConsole.WriteExperiment(
            5,
            "Teste automatizado",
            "Os testes validam limite de concorrência, fan-out, PLINQ, primos e cancelamento.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: os testes travam o contrato didático do paralelismo.",
            "tests/CSharpCodePortfolio.Tutorials.Tutorial28.Tests/ParallelismScenarioTests.cs",
            new CodeExcerpt(13, 24, "Contrato completo do relatório"),
            new CodeExcerpt(30, 36, "Limite de concorrência"),
            new CodeExcerpt(42, 46, "Fan-out com Task.WhenAll"),
            new CodeExcerpt(52, 54, "Grau de paralelismo inválido"));

        TutorialConsole.WriteObservation(
            "Paralelismo não é sinônimo de velocidade automática. O ganho aparece quando há trabalho independente suficiente e o limite de concorrência respeita o recurso usado.");
        TutorialConsole.WriteConclusion(
            "O modelo prático é escolher a ferramenta pelo tipo de trabalho: `Task.WhenAll` para fan-out assíncrono, `Parallel.ForEachAsync` para concorrência limitada, PLINQ para consultas puras e `Parallel.ForEach` para CPU bound.",
            TutorialConclusionKind.Success);
    }
}
