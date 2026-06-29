using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of the command-side aggregate writer.
/// </summary>
public sealed class EfUserAccountWriter(RegistrationDbContext dbContext) : IUserAccountWriter
{
    /// <summary>
    /// Adds the aggregate, commits the unit of work, and clears captured domain events after a successful save.
    /// </summary>
    public async Task AddAsync(UserAccount account, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(account, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        account.ClearDomainEvents();
    }
}
