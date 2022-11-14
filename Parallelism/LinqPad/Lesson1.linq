<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	var action1 = new Action(Processo1);
	var action2 = new Action(Processo2);
	var action3 = new Action(Processo3);
	var stopWatch = new Stopwatch();
	stopWatch.Start();
	Parallel.Invoke(action1, action2, action3);
	stopWatch.Stop();
	Console.WriteLine($"O tempo de processamento total é de {stopWatch.ElapsedMilliseconds} ms");
	
}

void Processo1() 
{	
	Console.WriteLine($"Processo1 Finalizado! Thread:{Thread.CurrentThread.ManagedThreadId}");
	Thread.Sleep(1000);
}

void Processo2()
{
	Console.WriteLine($"Processo2 Finalizado! Thread:{Thread.CurrentThread.ManagedThreadId}");
	Thread.Sleep(1000);
}

void Processo3()
{
	Console.WriteLine($"Processo3 Finalizado! Thread:{Thread.CurrentThread.ManagedThreadId}");
	Thread.Sleep(1000);
}

// You can define other methods, fields, classes and namespaces here