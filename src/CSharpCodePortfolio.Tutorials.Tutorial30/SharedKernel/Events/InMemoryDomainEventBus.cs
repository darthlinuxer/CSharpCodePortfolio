using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;

public interface IInMemoryDomainEventBus
{
    Task<Either<Seq<DomainError>, Unit>> PublishAsync(
        Seq<IDomainEvent> domainEvents,
        CancellationToken cancellationToken);
}

public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task<Either<Seq<DomainError>, Unit>> HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}

public interface IDomainEventConsumer
{
    Type DomainEventType { get; }

    Task<Either<Seq<DomainError>, Unit>> HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}

public sealed class DomainEventConsumer<TDomainEvent>(IDomainEventHandler<TDomainEvent> handler) : IDomainEventConsumer
    where TDomainEvent : IDomainEvent
{
    public Type DomainEventType => typeof(TDomainEvent);

    public Task<Either<Seq<DomainError>, Unit>> HandleAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken) =>
        handler.HandleAsync((TDomainEvent)domainEvent, cancellationToken);
}

public sealed class InMemoryDomainEventBus(IEnumerable<IDomainEventConsumer> consumers) : IInMemoryDomainEventBus
{
    private readonly IReadOnlyDictionary<Type, Seq<IDomainEventConsumer>> consumersByEventType =
        consumers
            .GroupBy(consumer => consumer.DomainEventType)
            .ToDictionary(group => group.Key, group => group.ToSeq());

    public async Task<Either<Seq<DomainError>, Unit>> PublishAsync(
        Seq<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        var results = new List<Either<Seq<DomainError>, Unit>>();

        foreach (var domainEvent in domainEvents)
        {
            foreach (var consumer in ConsumersFor(domainEvent))
            {
                results.Add(await consumer.HandleAsync(domainEvent, cancellationToken).ConfigureAwait(false));
            }
        }

        var errors = results
            .SelectMany(result => result.Match(Right: _ => Enumerable.Empty<DomainError>(), Left: value => value))
            .ToSeq();

        return errors.IsEmpty
            ? Right<Seq<DomainError>, Unit>(default)
            : Left<Seq<DomainError>, Unit>(errors);
    }

    private Seq<IDomainEventConsumer> ConsumersFor(IDomainEvent domainEvent) =>
        consumersByEventType.TryGetValue(domainEvent.GetType(), out var handlers)
            ? handlers
            : Seq<IDomainEventConsumer>();
}
