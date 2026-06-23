namespace CSharpCodePortfolio.Tutorials.Tutorial28;

internal sealed class ConcurrencyProbe
{
    private int current;
    private int maxObserved;

    public int MaxObserved => Volatile.Read(ref maxObserved);

    public int Enter()
    {
        var now = Interlocked.Increment(ref current);
        var snapshot = Volatile.Read(ref maxObserved);
        while (now > snapshot)
        {
            var original = Interlocked.CompareExchange(ref maxObserved, now, snapshot);
            if (original == snapshot)
            {
                break;
            }

            snapshot = original;
        }

        return now;
    }

    public void Exit()
    {
        _ = Interlocked.Decrement(ref current);
    }
}
