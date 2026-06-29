using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Queries;

/// <summary>
/// EF Core implementation of the user lookup query port.
/// </summary>
public sealed class EfUserAccountLookup(RegistrationDbContext dbContext) : IUserAccountLookup
{
    /// <summary>
    /// Checks persisted users for an existing normalized document.
    /// </summary>
    public Task<bool> DocumentExistsAsync(string document, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Document == document, cancellationToken);
    }

    /// <summary>
    /// Checks persisted users for an existing required email.
    /// </summary>
    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken)
    {
        // ponytail: EF InMemory does not translate predicates over EF Core 10 complex properties yet; query the synced scalar.
        return dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.EmailLookupValue == email.Value, cancellationToken);
    }

    /// <summary>
    /// Reads a user projection by identity without exposing the tracked aggregate.
    /// </summary>
    public async Task<Option<UserAccountQueryDto>> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return user is null
            ? None
            : Some(new UserAccountQueryDto(
                user.Id,
                user.Name.Value,
                user.Document,
                user.Email.Value,
                user.PhoneNumber.Map(phone => phone.Value)));
    }
}
