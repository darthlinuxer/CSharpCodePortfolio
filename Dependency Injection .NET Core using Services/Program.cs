using System;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
	public static IServiceProvider serviceProvider;

	public static void Configure()
	{
		var instance = new DefinedValuedServiceClass { Value = 10 }; //creating a service object with a defined initial value property
		serviceProvider = new ServiceCollection()
			.AddSingleton<IDemoService, DemoService>() //Create a Singleton Service that justs writes a text when its method is called
			.AddSingleton<ITestService, MyTestService>() //Create and add a nested Singleton Service that depends on DemoService
			.AddTransient<TransientOperation>() //Create a Simple Transient Service that writes a text on creation
			.AddScoped<ScopedOperation>() //Create a Simple Scoped Service  that writes a text on creation
			.AddSingleton<SingletonOperation>() //Create a Simple Singleton Service  that writes a text on creation
			.AddSingleton(instance) //Add a Service Object with an already initialized value
			.AddTransient(typeof(GenericRepository<>)) //Add a Generic Service 
			.AddSingleton<IService, ServiceA>() //Adding multiple services with same type
			.AddSingleton<IService, ServiceB>() //Adding multiple services with same type
			.BuildServiceProvider();
	}

	public static void Main()
	{
		Configure();
		Console.WriteLine("Initiating Service Operations");
		Console.WriteLine("\n-------- First Request --------");
		//With the scope the aim is to see if after the scope, which service will be destroyed
		using (var scope = serviceProvider.CreateScope())
		{
			var singletonService = serviceProvider.GetService<SingletonOperation>();
			var scopedService = serviceProvider.GetService<ScopedOperation>();
			var transientService = serviceProvider.GetService<TransientOperation>();
		} //Why the disposed methods of Scoped and Transient Services are not being called ?

		//Get Nested Service on Local Scope and Execution its method. 
		var test = serviceProvider.GetService<ITestService>();
		test.TestService();

		//Returning a service with an already initialized value and printing its value
		var myReturnedInstance = serviceProvider.GetService<DefinedValuedServiceClass>();
		Console.WriteLine($"Initialized valued on service was: {myReturnedInstance.Value} ");

		//returning a Generic Type Service and executing its Add method
		var genericClientService = (GenericRepository<Client>)serviceProvider.GetService(typeof(GenericRepository<Client>));
		var client = new Client { name = "Camilo" };
		genericClientService.Add(client);

		//Returning multiple services with the same type and also, getting the last added type
		var multipleServices = serviceProvider.GetServices<IService>();
		var lastAddedService = serviceProvider.GetService<IService>();
		foreach (var service in multipleServices)
		{
			Console.WriteLine(service.SaySomething());
		}

		Console.WriteLine(lastAddedService.SaySomething());
		Console.WriteLine("\n-------- Second Request --------");
		using (var scope = serviceProvider.CreateScope()) //adding the same singleton, scope and transient services agains
		{
			var singletonService = serviceProvider.GetService<SingletonOperation>();
			var scopedService = serviceProvider.GetService<ScopedOperation>();
			var transientService = serviceProvider.GetService<TransientOperation>();
		} //Why the disposed methods of Scoped and Transient Services are not being called ?

		Console.WriteLine();
		Console.WriteLine(new String('-', 30));
		Console.WriteLine("Operations Concluded!");
		Console.ReadLine();
	}
}

//DEFINING THE SERVICES
public interface IDemoService { void ServiceDemo(int number); }
public interface ITestService { void TestService(); }

public class DemoService : IDemoService
{
	public void ServiceDemo(int number) => Console.WriteLine($"Executing DemoService Operation number: {number} ");
}

public class MyTestService : ITestService
{
	private readonly IDemoService _demoService;
	public MyTestService(IDemoService demoService)
	{
		_demoService = demoService;
	} //Nested Service on Constructor. 

	public void TestService()
	{
		for (int i = 0; i <= 1; i++)
		{
			_demoService.ServiceDemo(i);
		}
	}
}

public class SingletonOperation : IDisposable
{
	private bool _disposed = false;
	public SingletonOperation() => Console.WriteLine("Singleton Service created!");
	public void Dispose()
	{
		if (_disposed)
			return;
		Console.WriteLine("SingletonService Disposed!");
		_disposed = true;
	}

	~SingletonOperation() => Dispose();
}

public class ScopedOperation : IDisposable
{
	private bool _disposed = false;
	public ScopedOperation() => Console.WriteLine("Scoped Service created!");
	public void Dispose()
	{
		if (_disposed)
			return;
		Console.WriteLine("ScopedService Disposed!");
		_disposed = true;
	}

	~ScopedOperation() => Dispose();
}

public class TransientOperation : IDisposable
{
	private bool _disposed = false;
	public TransientOperation() => Console.WriteLine("Transient Service created!");
	public void Dispose()
	{
		if (_disposed)
			return;
		Console.WriteLine("TransientService Disposed!");
		_disposed = true;
	}

	~TransientOperation() => Dispose();
}

public class DefinedValuedServiceClass { public int Value { get; set; } }

public class GenericRepository<T> { public void Add(T obj) => Console.WriteLine($"Adding object of type {obj.GetType()}"); }

public interface IService { public string SaySomething(); }

public class ServiceA : IService { public string SaySomething() => "I am Service A"; }

public class ServiceB : IService { public string SaySomething() => "I am Service B"; }

//CLASSES
public class Client { public string name { get; set; } }
