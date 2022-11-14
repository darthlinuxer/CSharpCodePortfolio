<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	var primes = new List<int>();
	var x = Enumerable.Range(1,50000);
	var maxPrimes = x.AsParallel();
	Parallel.ForEach(maxPrimes, new ParallelOptions()
	{
		MaxDegreeOfParallelism = 8
	}, n =>
	{
		var result = PrimeGenerator.FindAllPrimes(1,n);
		lock(primes) primes.Add(result.Count());
	});
	var orderedPrimes = primes.OrderBy(c=>c).ToList();
	x.Chart().AddYSeries(orderedPrimes, LINQPad.Util.SeriesType.Line).Dump();	
}

public class PrimeGenerator
{
	public static List<int> FindAllPrimes(int min, int max)
	{
		var primes = new List<int>();
		var collection = Enumerable.Range(min, max - min).AsParallel();
		Parallel.ForEach(collection, new ParallelOptions()
		{
			MaxDegreeOfParallelism = 8
		}, n => { if (IsPrime(n)) { lock (primes) primes.Add(n); } });
		return primes;
	}

	private static bool IsPrime(int n)
	{
		if (n <= 3 && n > 1) return true;
		if (n == 1 || n % 2 == 0 || n % 3 == 0) return false;
		uint i;
		for (i = 5; i * i <= n; i += 6)
		{
			if (n % i == 0 || n % (i + 2) == 0)	return false;
		}
		return true;
	}

}



// You can define other methods, fields, classes and namespaces here