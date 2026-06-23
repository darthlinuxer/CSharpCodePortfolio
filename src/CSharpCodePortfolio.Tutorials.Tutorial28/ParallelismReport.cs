namespace CSharpCodePortfolio.Tutorials.Tutorial28;

internal sealed record ParallelismReport(
    IReadOnlyList<string> IndependentActions,
    ParallelRun Sequential,
    ParallelRun FanOut,
    ParallelRun Limited,
    PlinqPartition Partition,
    IReadOnlyList<int> Primes,
    bool CancellationObserved,
    IReadOnlyList<string> Checklist);
