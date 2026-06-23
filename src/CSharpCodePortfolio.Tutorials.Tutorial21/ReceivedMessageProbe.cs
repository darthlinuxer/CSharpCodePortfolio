namespace CSharpCodePortfolio.Tutorials.Tutorial21;

internal sealed class ReceivedMessageProbe
{
    private readonly TaskCompletionSource<PortfolioMessage> firstMessage = new(
        TaskCreationOptions.RunContinuationsAsynchronously);

    private int receivedCount;

    public int ReceivedCount => Volatile.Read(ref receivedCount);

    public void Record(PortfolioMessage message)
    {
        Interlocked.Increment(ref receivedCount);
        firstMessage.TrySetResult(message);
    }

    public Task<PortfolioMessage> WaitForMessageAsync(CancellationToken cancellationToken)
    {
        return firstMessage.Task.WaitAsync(cancellationToken);
    }
}
