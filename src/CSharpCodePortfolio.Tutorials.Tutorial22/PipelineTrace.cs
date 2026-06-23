namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed class PipelineTrace
{
    private readonly object gate = new();
    private readonly List<string> events = [];

    public void Record(string item)
    {
        lock (gate)
        {
            events.Add(item);
        }
    }

    public IReadOnlyList<string> SnapshotAndClear()
    {
        lock (gate)
        {
            var snapshot = events.ToArray();
            events.Clear();
            return snapshot;
        }
    }
}
