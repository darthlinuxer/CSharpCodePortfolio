namespace CSharpCodePortfolio.Tutorials.Tutorial28;

internal sealed record ParallelRun(
    string Strategy,
    long ElapsedMilliseconds,
    int MaxConcurrency,
    int TotalScore,
    IReadOnlyList<WorkResult> Results)
{
    public static ParallelRun From(string strategy, long elapsedMilliseconds, int maxConcurrency, IEnumerable<WorkResult> results)
    {
        var orderedResults = results
            .OrderBy(static result => result.Name)
            .ToArray();

        return new ParallelRun(
            strategy,
            elapsedMilliseconds,
            maxConcurrency,
            orderedResults.Sum(static result => result.Score),
            orderedResults);
    }
}
