namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;

/// <summary>
/// Application transaction boundary backed by EF Core in infrastructure.
/// </summary>
public interface IRegistrationUnitOfWork
{
    /// <summary>
    /// Commits tracked changes and returns the number of affected entries.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
