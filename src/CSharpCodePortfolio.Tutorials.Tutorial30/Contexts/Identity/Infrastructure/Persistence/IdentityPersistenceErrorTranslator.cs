using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Infrastructure.Persistence;

public sealed class IdentityPersistenceErrorTranslator : IEfPersistenceErrorTranslator
{
    public async Task<Option<Seq<DomainError>>> TranslateAsync(
        DbUpdateException exception,
        Tutorial30DbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(dbContext);

        var accounts = dbContext.ChangeTracker
            .Entries<UserAccount>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .ToArray();
        var duplicatedEmails = accounts
            .GroupBy(account => account.Email)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        var errors = new List<DomainError>();

        foreach (var account in accounts)
        {
            var persistedDuplicate = await dbContext.Users.AsNoTracking()
                    .AnyAsync(user => user.Id != account.Id && user.Email == account.Email, cancellationToken)
                    .ConfigureAwait(false);
            var trackedDuplicate = duplicatedEmails.Contains(account.Email);

            _ = (persistedDuplicate || trackedDuplicate)
                ? Add(errors, new UserAccountEmailDuplicateError())
                : default;
        }

        return errors.Count == 0
            ? None
            : Some(errors.DistinctBy(error => error.GetType()).ToSeq());
    }

    private static Unit Add(List<DomainError> errors, DomainError error)
    {
        errors.Add(error);
        return default;
    }
}
