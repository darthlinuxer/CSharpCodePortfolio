namespace CSharpCodePortfolio.Tutorials.Tutorial14;

internal sealed record ClientReport(
    IReadOnlyList<Client> AfterSeed,
    IReadOnlyList<Client> AfterInsert,
    Client? UpdatedClient,
    IReadOnlyList<Client> SearchResult,
    bool Removed,
    IReadOnlyList<Client> FinalClients);
