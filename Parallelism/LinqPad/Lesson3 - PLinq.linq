<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

void Main()
{
	var lista1 = Enumerable.Range(0, 10).AsParallel();
	var lista2 = Enumerable.Range(0,10).AsParallel();

	var options = new ParallelOptions()
	{
		MaxDegreeOfParallelism = 4
	};

	var stopWatch = new Stopwatch();
	stopWatch.Start();
	var result1 = lista1.Where(n => n%2==0);
	var result2 = from n in lista2
				  where n %2 == 1
				  select n;

	Parallel.ForEach(result1, options, i =>
	{
		Thread.Sleep(1000);
		Console.WriteLine($"Thread:{Thread.CurrentThread.ManagedThreadId} even number: {i}");
	});

	Parallel.ForEach(result2, options, i =>
	{
		Thread.Sleep(1000);
		Console.WriteLine($"Thread:{Thread.CurrentThread.ManagedThreadId} odd number: {i}");
	});

	stopWatch.Stop();
	Console.WriteLine($"Elapsed Time: {stopWatch.ElapsedMilliseconds} ms");

}