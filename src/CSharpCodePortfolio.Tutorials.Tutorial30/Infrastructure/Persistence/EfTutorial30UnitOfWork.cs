using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

public sealed class EfTutorial30UnitOfWork(
    Tutorial30DbContext dbContext,
    IInMemoryDomainEventBus domainEventBus,
    ITransactionalExecution transactionalExecution,
    IEnumerable<IEfPersistenceErrorTranslator> persistenceErrorTranslators) : IUnitOfWork
{
    private const int MaxDomainEventDispatchCycles = 16;

    public async Task<Either<Seq<DomainError>, PersistenceResult>> SaveEntitiesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var publishedEvents = new System.Collections.Generic.HashSet<IDomainEvent>(ReferenceEqualityComparer.Instance);
            var result = await transactionalExecution
                .ExecuteAsync(token => SaveEntitiesCoreAsync(publishedEvents, token), cancellationToken)
                .ConfigureAwait(false);

            return result.Match(
                Right: value => Right<Seq<DomainError>, PersistenceResult>(
                    ClearEventsAndReturn(value)),
                Left: errors => Left<Seq<DomainError>, PersistenceResult>(
                    DiscardTrackedStateAndReturn(errors)));
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            var errors = await TranslateErrorsAsync(exception, cancellationToken).ConfigureAwait(false);

            return Left<Seq<DomainError>, PersistenceResult>(DiscardTrackedStateAndReturn(errors));
        }
    }

    private async Task<Either<Seq<DomainError>, PersistenceResult>> SaveEntitiesCoreAsync(
        System.Collections.Generic.HashSet<IDomainEvent> publishedEvents,
        CancellationToken cancellationToken)
    {
        var savedRows = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await DrainDomainEventsAsync(
            publishedEvents,
            savedRows,
            0,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, PersistenceResult>> DrainDomainEventsAsync(
        System.Collections.Generic.HashSet<IDomainEvent> publishedEvents,
        int rowsAffected,
        int cycle,
        CancellationToken cancellationToken)
    {
        var pendingEvents = PendingDomainEvents(publishedEvents);

        return pendingEvents.IsEmpty
            ? Right<Seq<DomainError>, PersistenceResult>(new PersistenceResult(rowsAffected))
            : await DispatchDomainEventsAsync(publishedEvents, pendingEvents, rowsAffected, cycle, cancellationToken)
                .ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, PersistenceResult>> DispatchDomainEventsAsync(
        System.Collections.Generic.HashSet<IDomainEvent> publishedEvents,
        Seq<IDomainEvent> pendingEvents,
        int rowsAffected,
        int cycle,
        CancellationToken cancellationToken) =>
        cycle >= MaxDomainEventDispatchCycles
            ? Left<Seq<DomainError>, PersistenceResult>(
                Seq1<DomainError>(new DomainEventDispatchCycleLimitExceededError(MaxDomainEventDispatchCycles)))
            : await PublishAndContinueAsync(publishedEvents, pendingEvents, rowsAffected, cycle, cancellationToken)
                .ConfigureAwait(false);

    private async Task<Either<Seq<DomainError>, PersistenceResult>> PublishAndContinueAsync(
        System.Collections.Generic.HashSet<IDomainEvent> publishedEvents,
        Seq<IDomainEvent> pendingEvents,
        int rowsAffected,
        int cycle,
        CancellationToken cancellationToken)
    {
        pendingEvents.Iter(domainEvent => publishedEvents.Add(domainEvent));
        var published = await domainEventBus.PublishAsync(pendingEvents, cancellationToken).ConfigureAwait(false);

        return await published.Match(
            Right: _ => SaveEffectsAndContinueAsync(publishedEvents, rowsAffected, cycle, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, PersistenceResult>(errors)))
            .ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, PersistenceResult>> SaveEffectsAndContinueAsync(
        System.Collections.Generic.HashSet<IDomainEvent> publishedEvents,
        int rowsAffected,
        int cycle,
        CancellationToken cancellationToken)
    {
        var publishedRows = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await DrainDomainEventsAsync(
            publishedEvents,
            rowsAffected + publishedRows,
            cycle + 1,
            cancellationToken).ConfigureAwait(false);
    }

    private IAggregate[] TrackedAggregatesWithEvents() =>
        dbContext.ChangeTracker.Entries()
            .Select(entry => entry.Entity)
            .OfType<IAggregate>()
            .Where(aggregate => aggregate.HasDomainEvents)
            .ToArray();

    private Seq<IDomainEvent> PendingDomainEvents(System.Collections.Generic.HashSet<IDomainEvent> publishedEvents) =>
        TrackedAggregatesWithEvents()
            .SelectMany(aggregate => aggregate.RecordedDomainEvents)
            .Where(domainEvent => !publishedEvents.Contains(domainEvent))
            .ToSeq();

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<Seq<DomainError>> TranslateErrorsAsync(
        DbUpdateException exception,
        CancellationToken cancellationToken)
    {
        var errors = new List<DomainError>();

        foreach (var translator in persistenceErrorTranslators)
        {
            _ = (await translator.TranslateAsync(exception, dbContext, cancellationToken).ConfigureAwait(false))
                .Match(
                    Some: value => AddRange(errors, value),
                    None: () => default);
        }

        return errors.Count == 0
            ? Seq1<DomainError>(new PersistenceConflictError())
            : errors.DistinctBy(error => error.GetType()).ToSeq();
    }

    private PersistenceResult ClearEventsAndReturn(PersistenceResult result)
    {
        TrackedAggregatesWithEvents().Iter(aggregate => aggregate.ClearDomainEvents());
        return result;
    }

    private Seq<DomainError> DiscardTrackedStateAndReturn(Seq<DomainError> errors)
    {
        dbContext.ChangeTracker.Clear();
        return errors;
    }

    private static Unit AddRange(List<DomainError> errors, Seq<DomainError> values)
    {
        errors.AddRange(values);
        return default;
    }
}
