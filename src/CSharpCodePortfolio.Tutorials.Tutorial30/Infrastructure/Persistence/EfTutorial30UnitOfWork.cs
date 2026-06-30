using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Persistence;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

public sealed class EfTutorial30UnitOfWork(
    Tutorial30DbContext dbContext,
    IInMemoryDomainEventBus domainEventBus) : ITutorial30UnitOfWork
{
    public async Task<Either<Seq<DomainError>, int>> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await CommitCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            var errors = await DetectUniqueConstraintErrorsAsync(cancellationToken).ConfigureAwait(false);
            DetachFailedInserts();

            return Left<Seq<DomainError>, int>(errors);
        }
    }

    private async Task<Either<Seq<DomainError>, int>> CommitCoreAsync(CancellationToken cancellationToken)
    {
        var aggregatesWithEvents = TrackedAggregatesWithEvents();
        var domainEvents = aggregatesWithEvents.SelectMany(aggregate => aggregate.RecordedDomainEvents).ToSeq();

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var savedRows = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var published = await domainEventBus.PublishAsync(domainEvents, cancellationToken).ConfigureAwait(false);

        return await published.Match(
            Right: _ => CommitPublishedAsync(transaction, savedRows, aggregatesWithEvents, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, int>(errors))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, int>> CommitPublishedAsync(
        IDbContextTransaction transaction,
        int savedRows,
        IAggregate[] aggregatesWithEvents,
        CancellationToken cancellationToken)
    {
        var publishedRows = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        aggregatesWithEvents.Iter(aggregate => aggregate.ClearDomainEvents());

        return Right<Seq<DomainError>, int>(savedRows + publishedRows);
    }

    private IAggregate[] TrackedAggregatesWithEvents() =>
        dbContext.ChangeTracker.Entries()
            .Select(entry => entry.Entity)
            .OfType<IAggregate>()
            .Where(aggregate => aggregate.HasDomainEvents)
            .ToArray();

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<Seq<DomainError>> DetectUniqueConstraintErrorsAsync(CancellationToken cancellationToken)
    {
        var accounts = dbContext.ChangeTracker
            .Entries<UserAccount>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .ToArray();

        var errors = new List<DomainError>();

        foreach (var account in accounts)
        {
            _ = (await dbContext.Users.AsNoTracking()
                    .AnyAsync(user => user.Id != account.Id && user.Email == account.Email, cancellationToken)
                    .ConfigureAwait(false))
                ? Add(errors, new UserAccountEmailDuplicateError())
                : default;
        }

        return errors.Count == 0
            ? Seq1<DomainError>(new UserAccountRegistrationConflictError())
            : errors.DistinctBy(error => error.GetType()).ToSeq();
    }

    private void DetachFailedInserts()
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<UserAccount>().Where(entry => entry.State == EntityState.Added))
        {
            entry.State = EntityState.Detached;
        }
    }

    private static Unit Add(List<DomainError> errors, DomainError error)
    {
        errors.Add(error);
        return default;
    }
}
