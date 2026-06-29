using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

/// <summary>
/// EF Core context used by the tutorial to prove async persistence without a real database server.
/// </summary>
public sealed class RegistrationDbContext(DbContextOptions<RegistrationDbContext> options)
    : DbContext(options), IRegistrationUnitOfWork
{
    /// <summary>
    /// Gets the user table used by the registration sample.
    /// </summary>
    public DbSet<UserAccount> Users => Set<UserAccount>();

    /// <summary>
    /// Commits through the application port and translates expected unique-key conflicts to typed errors.
    /// </summary>
    public async Task<Either<Seq<DomainError>, int>> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Right<Seq<DomainError>, int>(result);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            var errors = await DetectUniqueConstraintErrorsAsync(cancellationToken).ConfigureAwait(false);
            DetachFailedInserts();

            return Left<Seq<DomainError>, int>(errors);
        }
    }

    /// <summary>
    /// Applies tutorial mappings that keep persistence concerns outside the aggregate.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegistrationDbContext).Assembly);

    /// <summary>
    /// Commits tracked changes and clears domain events only after EF Core persists successfully.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<IEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => !entity.DomainEvents.IsEmpty)
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return result;
    }

    /// <summary>
    /// Detects provider messages that represent unique index violations.
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Narrows a unique-key exception back to typed user registration errors.
    /// </summary>
    private async Task<Seq<DomainError>> DetectUniqueConstraintErrorsAsync(CancellationToken cancellationToken)
    {
        var accounts = ChangeTracker
            .Entries<UserAccount>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .ToArray();

        var errors = new List<DomainError>();

        foreach (var account in accounts)
        {
            if (await Users.AsNoTracking()
                    .AnyAsync(user => user.Id != account.Id && user.Document == account.Document, cancellationToken)
                    .ConfigureAwait(false))
            {
                errors.Add(new UserAccountDocumentDuplicateError());
            }

            if (await Users.AsNoTracking()
                    .AnyAsync(user => user.Id != account.Id && user.Email == account.Email, cancellationToken)
                    .ConfigureAwait(false))
            {
                errors.Add(new UserAccountEmailDuplicateError());
            }
        }

        return errors.Count == 0
            ? Seq1<DomainError>(new UserAccountRegistrationConflictError())
            : errors.DistinctBy(error => error.GetType()).ToSeq();
    }

    /// <summary>
    /// Removes failed inserts from tracking so a returned Left does not poison the current context.
    /// </summary>
    private void DetachFailedInserts()
    {
        foreach (var entry in ChangeTracker.Entries<UserAccount>().Where(entry => entry.State == EntityState.Added))
        {
            entry.State = EntityState.Detached;
        }
    }
}
