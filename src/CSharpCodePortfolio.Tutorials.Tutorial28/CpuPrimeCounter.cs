using System.Collections.Concurrent;

namespace CSharpCodePortfolio.Tutorials.Tutorial28;

internal sealed class CpuPrimeCounter
{
    public IReadOnlyList<int> FindPrimesUntil(int inclusiveMax, int maxDegreeOfParallelism, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(inclusiveMax, 2);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        var primes = new ConcurrentBag<int>();
        var options = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        Parallel.ForEach(Enumerable.Range(2, inclusiveMax - 1), options, number =>
        {
            if (IsPrime(number))
            {
                primes.Add(number);
            }
        });

        return primes.OrderBy(static number => number).ToArray();
    }

    public static bool IsPrime(int number)
    {
        if (number <= 1)
        {
            return false;
        }

        if (number <= 3)
        {
            return true;
        }

        if (number % 2 == 0 || number % 3 == 0)
        {
            return false;
        }

        for (var divisor = 5; divisor * divisor <= number; divisor += 6)
        {
            if (number % divisor == 0 || number % (divisor + 2) == 0)
            {
                return false;
            }
        }

        return true;
    }
}
