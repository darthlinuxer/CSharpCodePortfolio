namespace CSharpCodePortfolio.Tutorials.Tutorial14;

internal sealed class ClientUnitOfWork(ClientDbContext context)
{
    private IRepository<Client, int>? clients;

    public IRepository<Client, int> Clients =>
        clients ??= new EfRepository<Client, int>(context);

    public Task<int> CommitAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
