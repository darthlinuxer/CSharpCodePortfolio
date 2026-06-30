using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of the command-side aggregate writer.
/// </summary>
public sealed class EfUserAccountWriter(Tutorial30DbContext dbContext) : IUserAccountWriter
{
    /// <summary>
    /// Adds the aggregate to EF Core tracking; commit belongs to the unit of work.
    /// </summary>
    public void Add(UserAccount account)
    {
        dbContext.Users.Add(account);
    }
}
