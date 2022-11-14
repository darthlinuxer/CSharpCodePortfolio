<Query Kind="Program">
  <NuGetReference>BenchmarkDotNet</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
  <Namespace>BenchmarkDotNet.Engines</Namespace>
  <Namespace>BenchmarkDotNet.Configs</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

#LINQPad optimize+
void Main()
{
	BenchmarkRunner.Run<BenchmarkParallelMethods>();
}

// You can define other methods, fields, classes and namespaces here
[SimpleJob(RunStrategy.ColdStart, targetCount: 5)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class BenchmarkParallelMethods
{
	[GlobalSetup]
	public void GlobalSetup()
	{

	}
	
	[BenchmarkCategory("Http"), Benchmark(Baseline = true)]
	public void SingleCoreParallelVersion()=> ParallelHttpCall(1);
	
	[BenchmarkCategory("Http"), Benchmark]
	public void DoubleCoreParallelVersion()=> ParallelHttpCall(2);

	[BenchmarkCategory("Http"), Benchmark]
	public void QuadCoreParallelVersion() => ParallelHttpCall(4);

	[BenchmarkCategory("Http"), Benchmark]
	public void UnimitedParallelVersion() => ParallelHttpCall(-1);


	private void ParallelHttpCall(int cores)
	{
		var collection = Enumerable.Range(1, 5).AsParallel();
		Parallel.ForEach(collection, new ParallelOptions()
		{
			MaxDegreeOfParallelism = cores
		}, _ =>
		{
			var user = GetUser();
			Console.WriteLine("Username: " + user["username"]);
		});
	}

	private IDictionary<string, object> GetUser()
	{
		var baseCep = $"https://random-data-api.com/api/v2/users?size=1";
		using var client = new HttpClient();
		var response = client.GetAsync(baseCep).Result;
		var content = response.Content.ReadAsStringAsync().Result;
		var result = JsonSerializer.Deserialize<IDictionary<string, object>>(content);
		return result;
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		// Disposing logic
	}
}
