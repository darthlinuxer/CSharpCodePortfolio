namespace CSharpCodePortfolio.Tutorials.Tutorial28;

internal sealed class SimulatedRemoteService(ConcurrencyProbe probe)
{
    public async Task<WorkResult> FetchAsync(WorkItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        var concurrency = probe.Enter();
        try
        {
            await Task.Delay(item.DelayMilliseconds, cancellationToken);
            return new WorkResult(item.Name, item.Score, concurrency, Environment.CurrentManagedThreadId);
        }
        finally
        {
            probe.Exit();
        }
    }
}
