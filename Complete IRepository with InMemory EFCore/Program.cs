using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

//MODEL
public class Client { public int Id { get; set; } public string Name { get; set; } }
//CONTEXT
public class AppDbContext : DbContext
{
	public DbSet<Client> Clients { get; set; }
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	public AppDbContext() { }
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		//optionsBuilder.UseSqlServer(@"Data Source=src;Initial Catalog=dbClients;Integrated Security=True");
		//optionsBuilder.UseSqlServer(Configuration[“ConnectionStrings:MyDBConnectionString”]);
		optionsBuilder.UseInMemoryDatabase(databaseName: "dbClients");
	}
}

public interface IRepository<T> where T : class
{
	void Add(T entity);
	void Delete(T entity);
	void Update(T entity);
	IEnumerable<T> Get();
	IEnumerable<T> Get(Expression<Func<T, bool>> predicate);
	T GetById(Expression<Func<T, bool>> predicate);
}
public class Repository<T> : IRepository<T> where T : class
{
	private readonly AppDbContext _context;
	public Repository(AppDbContext context) => _context = context;
	public void Add(T entity) => _context.Set<T>().Add(entity);
	public void Delete(T entity) => _context.Set<T>().Remove(entity);
	public void Update(T entity) { _context.Entry(entity).State = EntityState.Modified; _context.Set<T>().Update(entity); }
	public IEnumerable<T> Get() => _context.Set<T>().AsEnumerable<T>();
	public IEnumerable<T> Get(Expression<Func<T, bool>> predicate) => _context.Set<T>().Where(predicate).AsEnumerable<T>();
	public T GetById(Expression<Func<T, bool>> predicate) => _context.Set<T>().SingleOrDefault(predicate);
}

public interface IUnitOfWork { IRepository<Client> ClientRepository { get; } void Commit(); }
public class UnitOfWork : IUnitOfWork, IDisposable
{
	public AppDbContext _context;
	private Repository<Client> _clientRepository;
	public UnitOfWork(AppDbContext context) => _context = context;
	public UnitOfWork() => _context = new AppDbContext();
	public IRepository<Client> ClientRepository { get => _clientRepository = _clientRepository ?? new Repository<Client>(_context); }
	public void Commit() => _context.SaveChanges();
	public void Dispose() => _context.Dispose();
}

//Business Logic
public interface IClientRepository : IRepository<Client> { IEnumerable<Client> GetClientsByName(); }

public class ClientRepository : IRepository<Client>, IClientRepository
{
	AppDbContext _context;
	public ClientRepository(AppDbContext context) => _context = context;
	public void Add(Client entity) => _context.Clients.Add(entity);
	public void Delete(Client entity) => _context.Clients.Remove(entity);
	public IEnumerable<Client> Get() => _context.Clients.ToList();
	public void Update(Client entity) => _context.Clients.Update(entity);
	public IEnumerable<Client> Get(Expression<Func<Client, bool>> predicate) => _context.Clients.Where(predicate);
	public Client GetById(Expression<Func<Client, bool>> predicate) => _context.Clients.FirstOrDefault(predicate);
	public IEnumerable<Client> GetClientsByName() => Get().OrderBy(c => c.Name).ToList();
}

public class ClientBusinessLogic : IDisposable
{
	UnitOfWork _uow;
	public ClientBusinessLogic(UnitOfWork uow) => _uow = uow;
	public ClientBusinessLogic() => _uow = new UnitOfWork();
	public IEnumerable<Client> ListClients() => _uow.ClientRepository.Get();
	public void AddClient(Client cli) { _uow.ClientRepository.Add(cli); _uow.Commit(); }
	public void DeleteClient(Client cli) { _uow.ClientRepository.Delete(cli); _uow.Commit(); }
	public void UpdateClient(Client cli) { _uow.ClientRepository.Update(cli); _uow.Commit(); }
	public Client GetClientById(int id) => _uow.ClientRepository.GetById(c => c.Id == id);
	public void Dispose() => _uow.Dispose();
}

public class Program
{
	public static void Main()
	{

		InitializeDbWithData();
		ListClients("Db list After Initialization");
		AddNewClient(1, "Camilo");
		ListClients("Db list After Adding a new Client");
		UpdateClient(1, "Camilo Chaves");
		ListClients("Db list After updating the Client name");
		DeleteClient(100);
		ListClients("Db list After Deleting a Client");
		Console.WriteLine("Name of Client with Id 1: " + GetClientById(1).Name);
	}

	private static void InitializeDbWithData()
	{
		AddNewClient(100, "John Rambo");
	}


	private static void AddNewClient(int id, string name)
	{
		using (var bll = new ClientBusinessLogic())
		{
			Client cli = new Client() { Id = id, Name = name };
			bll.AddClient(cli);
		}
	}

	private static void DeleteClient(int id)
	{
		using (var bll = new ClientBusinessLogic())
		{
			Client cli = bll.GetClientById(id);
			bll.DeleteClient(cli);
		}
	}

	private static void UpdateClient(int id, string name)
	{
		using (var bll = new ClientBusinessLogic())
		{
			Client cli = bll.GetClientById(id);
			cli.Name = name;
			bll.UpdateClient(cli);
		}
	}
	private static void ListClients(string msg)
	{
		using (var bll = new ClientBusinessLogic())
		{
			var clients = bll.ListClients();
			foreach (var cli in clients)
			{
				Console.WriteLine($"{msg}: Client -> Id:{cli.Id} , Name:{cli.Name}");
			}
		}
	}

	private static Client GetClientById(int Id)
	{
		using (var bll = new ClientBusinessLogic())
		{
			var client = bll.GetClientById(Id);
			return client;
		}
	}

}
