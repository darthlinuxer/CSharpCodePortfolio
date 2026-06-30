using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Persistence;

/// <summary>
/// Application persistence port for command-side user aggregate writes.
/// </summary>
public interface IUserAccountWriter
{
    /// <summary>
    /// Adds a new aggregate to the current unit of work.
    /// </summary>
    void Add(UserAccount account);

    Task<Option<UserAccount>> FindByIdAsync(Guid id, CancellationToken cancellationToken);
}
