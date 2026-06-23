using System.Collections.Concurrent;
using System.Diagnostics;

namespace CSharpCodePortfolio.Tutorials.Tutorial28;

internal static class ParallelismScenario
{
    public static async Task<ParallelismReport> RunAsync(CancellationToken cancellationToken = default)
    {
        var workItems = CreateWorkItems();
        var independentActions = RunIndependentActions();
        var sequential = await RunSequentialAsync(workItems, cancellationToken);
        var fanOut = await RunWithWhenAllAsync(workItems, cancellationToken);
        var limited = await RunWithForEachAsync(workItems, maxDegreeOfParallelism: 2, cancellationToken);
        var partition = PartitionWithPlinq();
        var primes = new CpuPrimeCounter().FindPrimesUntil(30, maxDegreeOfParallelism: 4, cancellationToken);
        var cancellationObserved = await ObserveCancellationAsync();

        return new ParallelismReport(
            independentActions,
            sequential,
            fanOut,
            limited,
            partition,
            primes,
            cancellationObserved,
            Checklist:
            [
                "Use Task.WhenAll quando todos os trabalhos assíncronos podem iniciar juntos.",
                "Use Parallel.ForEachAsync quando o fluxo precisa limitar concorrência e propagar CancellationToken.",
                "Use PLINQ para consultas puras sobre coleções em memória.",
                "Use Parallel.ForEach para trabalho de CPU com estado compartilhado protegido."
            ]);
    }

    public static IReadOnlyList<WorkItem> CreateWorkItems()
    {
        return
        [
            new WorkItem("Catálogo", 35, 2),
            new WorkItem("Preço", 35, 3),
            new WorkItem("Estoque", 35, 5),
            new WorkItem("Frete", 35, 7)
        ];
    }

    public static IReadOnlyList<string> RunIndependentActions()
    {
        var actions = new ConcurrentBag<string>();
        Parallel.Invoke(
            () => actions.Add("Calcular preço"),
            () => actions.Add("Reservar estoque"),
            () => actions.Add("Consultar frete"));

        return actions.OrderBy(static action => action).ToArray();
    }

    public static async Task<ParallelRun> RunSequentialAsync(IReadOnlyList<WorkItem> items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        var probe = new ConcurrencyProbe();
        var service = new SimulatedRemoteService(probe);
        var results = new List<WorkResult>(items.Count);
        var stopwatch = Stopwatch.StartNew();

        foreach (var item in items)
        {
            results.Add(await service.FetchAsync(item, cancellationToken));
        }

        stopwatch.Stop();
        return ParallelRun.From("Sequencial", stopwatch.ElapsedMilliseconds, probe.MaxObserved, results);
    }

    public static async Task<ParallelRun> RunWithWhenAllAsync(IReadOnlyList<WorkItem> items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        var probe = new ConcurrencyProbe();
        var service = new SimulatedRemoteService(probe);
        var stopwatch = Stopwatch.StartNew();
        var results = await Task.WhenAll(items.Select(item => service.FetchAsync(item, cancellationToken)));
        stopwatch.Stop();

        return ParallelRun.From("Task.WhenAll", stopwatch.ElapsedMilliseconds, probe.MaxObserved, results);
    }

    public static async Task<ParallelRun> RunWithForEachAsync(
        IReadOnlyList<WorkItem> items,
        int maxDegreeOfParallelism,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        var probe = new ConcurrencyProbe();
        var service = new SimulatedRemoteService(probe);
        var results = new ConcurrentBag<WorkResult>();
        var options = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };
        var stopwatch = Stopwatch.StartNew();

        await Parallel.ForEachAsync(items, options, async (item, token) =>
        {
            results.Add(await service.FetchAsync(item, token));
        });

        stopwatch.Stop();
        return ParallelRun.From("Parallel.ForEachAsync", stopwatch.ElapsedMilliseconds, probe.MaxObserved, results);
    }

    public static PlinqPartition PartitionWithPlinq()
    {
        var numbers = Enumerable.Range(0, 10);
        var even = numbers
            .AsParallel()
            .Where(static number => number % 2 == 0)
            .OrderBy(static number => number)
            .ToArray();
        var odd = numbers
            .AsParallel()
            .Where(static number => number % 2 == 1)
            .OrderBy(static number => number)
            .ToArray();

        return new PlinqPartition(even, odd);
    }

    private static async Task<bool> ObserveCancellationAsync()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        try
        {
            _ = await RunWithForEachAsync(CreateWorkItems(), maxDegreeOfParallelism: 2, source.Token);
            return false;
        }
        catch (OperationCanceledException)
        {
            return true;
        }
    }
}
