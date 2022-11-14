<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

void Main()
{
	string[] ceps = new string[]
	{
		"07155081", "15800100", "38407369","77445100","78015818","38300084","38300078"
	};
	
	var options = new ParallelOptions()
	{
		MaxDegreeOfParallelism = 2
	};
	
	var stopWatch = new Stopwatch();
	var results = new ConcurrentBag<string>();
	stopWatch.Start();
	ParallelLoopResult loopResult;
	loopResult = Parallel.ForEach(ceps, options, (string cep, ParallelLoopState state) =>
	{
		//Adding random break condition
		var rnd = new Random().Next(0, 100);
		if (rnd > 50) state.Break();
		if(state.ShouldExitCurrentIteration) return;		
		
		var result = new ViaCepService().GetCep(cep);
		results.Add(result.ToString());
		Console.WriteLine($"Rnd:{rnd} Thread: {Thread.CurrentThread.ManagedThreadId} -> {result}");
	});
	
	stopWatch.Stop();

	int i = 1;
	Console.WriteLine("\nOrdered Results");
	results.OrderBy(r => r).ToList().ForEach(cep => Console.WriteLine($"{i++} {cep}"));
	
	Console.WriteLine("\nStatistics:");
	
	Console.WriteLine($"All ParallelForEach executed completely : {loopResult.IsCompleted}");
	Console.WriteLine($"Lowest Break Interation : {loopResult.LowestBreakIteration}");
	Console.WriteLine($"Elapsed time: {stopWatch.ElapsedMilliseconds} ms");
}

public class CepModel
{
	[JsonPropertyName("cep")]
	public string Cep { get; set; }
	[JsonPropertyName("logradouro")]
	public string Logradouro { get; set; }
	[JsonPropertyName("complemento")]
	public string Complemento {get; set;}
	[JsonPropertyName("bairro")]
	public string Bairro { get; set; }
	[JsonPropertyName("localidade")]
	public string Localidade { get; set; }
	[JsonPropertyName("uf")]
	public string Uf {get; set;}
	[JsonPropertyName("ibge")]
	public string Ibge { get; set; }
	[JsonPropertyName("gia")]
	public string Gia {get; set;}
	[JsonPropertyName("ddd")]
	public string DDD {get; set;}
	[JsonPropertyName("siafi")]
	public string Siafi { get; set; }

	public override string ToString()
	{
		 return $"{this.Cep} - {this.Localidade}/{this.Uf}, {this.Bairro}";
	}
}

public class ViaCepService
{
	public CepModel? GetCep(string cep)
	{
		try
		{
			var baseCep = $"https://viacep.com.br/ws/{cep}/json";
			using var client = new HttpClient();
			var response = client.GetAsync(baseCep).Result;
			var content = response.Content.ReadAsStringAsync().Result;
			var cepResult = JsonSerializer.Deserialize<CepModel>(content);
			return cepResult;
		} catch 
		{
			return null;
		}
	}
}
