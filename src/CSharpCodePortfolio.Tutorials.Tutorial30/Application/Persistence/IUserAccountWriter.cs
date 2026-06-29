using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;

/// <summary>
/// Application persistence port for command-side user aggregate writes.
/// </summary>
public interface IUserAccountWriter
{
    /// <summary>
    /// Adds a new aggregate and commits the unit of work.
    /// </summary>
    Task AddAsync(UserAccount account, CancellationToken cancellationToken);
}
