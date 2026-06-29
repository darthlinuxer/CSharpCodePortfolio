using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;

/// <summary>
/// Application persistence port for command-side user aggregate writes.
/// </summary>
public interface IUserAccountWriter
{
    /// <summary>
    /// Adds a new aggregate to the current unit of work.
    /// </summary>
    void Add(UserAccount account);

    /// <summary>
    /// Removes an aggregate from the current unit of work.
    /// </summary>
    void Delete(UserAccount account);
}
