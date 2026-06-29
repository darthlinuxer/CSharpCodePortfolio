using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of the command-side aggregate writer.
/// </summary>
public sealed class EfUserAccountWriter(RegistrationDbContext dbContext) : IUserAccountWriter
{
    /// <summary>
    /// Adds the aggregate to EF Core tracking; commit belongs to the unit of work.
    /// </summary>
    public void Add(UserAccount account)
    {
        dbContext.Users.Add(account);
    }

    /// <summary>
    /// Marks the aggregate for deletion; commit belongs to the unit of work.
    /// </summary>
    public void Delete(UserAccount account)
    {
        dbContext.Users.Remove(account);
    }
}
