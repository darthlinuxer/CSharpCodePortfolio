using Microsoft.Extensions.Logging;

namespace CSharpCodePortfolio.Tutorials.Tutorial29;

internal class LoggingMemoryComparison
{
    public const int DefaultIterations = 1_000_000;

    public static LoggingComparisonResult Run<T>(
        ILogger<T> logger1,
        LoggerAdapter<T> logger2,
        int iterations = DefaultIterations)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(iterations);

        var payloadCounter1 = new PayloadCounter();
        var allocatedBytes = MeasureAllocatedBytes(
            () =>
            {
                for (var i = 0; i < iterations; i++)
                {
                    logger1.LogDebug("Using random numbers {1} {2} {3]}", Random.Shared.Next(), Random.Shared.Next(), Random.Shared.Next());
                }
            }
        );
        var result1 = new LoggingMemoryResult(allocatedBytes, payloadCounter1.FactoryCalls);

        var payloadCounter2 = new PayloadCounter();
        allocatedBytes = MeasureAllocatedBytes(
            () =>
            {
                for (var i = 0; i < iterations; i++)
                {
                    logger2.LogDebug("Using random numbers {1} {2} {3]}", Random.Shared.Next(), Random.Shared.Next(), Random.Shared.Next());
                }
            }
        );

        var result2 = new LoggingMemoryResult(allocatedBytes, payloadCounter2.FactoryCalls);

        return new LoggingComparisonResult(
            result1,
            result2,
            result1.allocatedBytes - result2.allocatedBytes);
    }

    private static long MeasureAllocatedBytes(Action action)
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    private static string CreateLargeDebugPayload(int sequence) => $"Debug payload {sequence}: {new string('x', 8_192)}";

    private static string CreateLargeDebugPayload(PayloadCounter counter)
    {
        counter.FactoryCalls++;
        return CreateLargeDebugPayload(counter.FactoryCalls);
    }

    private sealed class PayloadCounter
    {
        public int FactoryCalls { get; set; }
    }
}

internal sealed record LoggingMemoryResult(
    long allocatedBytes,
    int factoryCalls);

internal sealed record LoggingComparisonResult(
    LoggingMemoryResult result1,
    LoggingMemoryResult result2,
    long savedBytes
);
