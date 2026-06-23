namespace CSharpCodePortfolio.Tutorials.Tutorial11;

internal sealed class ClientCatalog(IRepository<Client, int> repository)
{
    public IReadOnlyList<Client> ListClients() =>
        repository.List().OrderBy(static client => client.Id).ToArray();

    public Client? GetClient(int id) =>
        repository.GetById(id);

    public void Register(Client client) =>
        repository.Add(client);

    public bool ChangeEmail(int id, string email)
    {
        var client = repository.GetById(id);
        return client is not null && repository.Update(client with { Email = email });
    }

    public bool Remove(int id) =>
        repository.Delete(id);
}
