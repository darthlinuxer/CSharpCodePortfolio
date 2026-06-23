namespace CSharpCodePortfolio.Tutorials.Tutorial14;

internal sealed class ClientCatalog(ClientUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<Client>> ListClientsAsync(CancellationToken cancellationToken)
    {
        var clients = await unitOfWork.Clients.ListAsync(cancellationToken);
        return clients.OrderBy(static client => client.Id).ToArray();
    }

    public Task<Client?> GetClientAsync(int id, CancellationToken cancellationToken) =>
        unitOfWork.Clients.GetByIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<Client>> FindClientsByNameAsync(
        string nameFragment,
        CancellationToken cancellationToken)
    {
        var clients = await unitOfWork.Clients.FindAsync(
            client => client.Name.Contains(nameFragment),
            cancellationToken);

        return clients.OrderBy(static client => client.Name).ToArray();
    }

    public async Task RegisterAsync(Client client, CancellationToken cancellationToken)
    {
        unitOfWork.Clients.Add(client);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task<bool> ChangeNameAsync(
        int id,
        string name,
        CancellationToken cancellationToken)
    {
        var client = await unitOfWork.Clients.GetByIdAsync(id, cancellationToken);
        if (client is null)
        {
            return false;
        }

        client.Name = name;
        unitOfWork.Clients.Update(client);
        await unitOfWork.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveAsync(int id, CancellationToken cancellationToken)
    {
        var client = await unitOfWork.Clients.GetByIdAsync(id, cancellationToken);
        if (client is null)
        {
            return false;
        }

        unitOfWork.Clients.Delete(client);
        await unitOfWork.CommitAsync(cancellationToken);
        return true;
    }
}
