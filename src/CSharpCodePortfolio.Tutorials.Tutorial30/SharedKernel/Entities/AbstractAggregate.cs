using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;

/// <summary>
/// Base class for aggregate roots that need identity, timestamp audit metadata, and domain events.
/// </summary>
/// <typeparam name="TAggregate">Aggregate type allowed to raise events through this entity.</typeparam>
/// <typeparam name="TId">Identity type used by the entity.</typeparam>
public abstract class AbstractAggregate<TAggregate, TId> : IAggregate<TAggregate, TId>
    where TId : notnull
{
    private readonly List<AbstractDomainEvent<TAggregate>> _domainEvents = [];
    private Option<Timestamp> _createdAt = None;
    private Option<Timestamp> _lastModified = None;

    /// <summary>
    /// Initializes an empty aggregate for EF Core materialization.
    /// </summary>
    protected AbstractAggregate()
    {
    }

    /// <summary>
    /// Initializes the entity with its stable identity.
    /// </summary>
    protected AbstractAggregate(TId id) => Id = id;

    /// <summary>
    /// Gets the aggregate identity.
    /// </summary>
    public TId Id { get; private set; } = default!;

    /// <summary>
    /// Gets the optional UTC creation timestamp.
    /// </summary>
    public Option<Timestamp> CreatedAt => _createdAt;

    /// <summary>
    /// Gets the optional UTC timestamp for the latest modification.
    /// </summary>
    public Option<Timestamp> LastModified => _lastModified;

    /// <summary>
    /// Gets events raised by completed domain behavior.
    /// </summary>
    public Seq<AbstractDomainEvent<TAggregate>> DomainEvents => _domainEvents.ToSeq();

    public bool HasDomainEvents => _domainEvents.Count > 0;

    /// <summary>
    /// Records creation metadata and the domain fact for a validated aggregate.
    /// </summary>
    protected Unit RecordCreated(Timestamp occurredAtUtc, Func<Timestamp, AbstractDomainEvent<TAggregate>> createEvent)
    {
        ArgumentNullException.ThrowIfNull(createEvent);

        _createdAt = Some(occurredAtUtc);
        AddDomainEvent(createEvent(occurredAtUtc));
        return default;
    }

    /// <summary>
    /// Applies a real state change and records its domain fact; no-op changes stay silent.
    /// </summary>
    protected Either<DomainError, Unit> ApplyChangeIfDifferent<TValue>(
        TValue current,
        TValue next,
        TimeProvider clock,
        Action<TValue> apply,
        Func<TValue, TValue, Timestamp, AbstractDomainEvent<TAggregate>> createEvent)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(apply);
        ArgumentNullException.ThrowIfNull(createEvent);

        return EqualityComparer<TValue>.Default.Equals(current, next)
            ? Right<DomainError, Unit>(default)
            : ApplyChange(current, next, clock, apply, createEvent);
    }

    /// <summary>
    /// Adds a domain event for effects that should happen after persistence.
    /// </summary>
    protected Unit AddDomainEvent(AbstractDomainEvent<TAggregate> domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
        return default;
    }

    /// <summary>
    /// Clears domain events after the persistence boundary has captured them.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Either<DomainError, Unit> ApplyChange<TValue>(
        TValue current,
        TValue next,
        TimeProvider clock,
        Action<TValue> apply,
        Func<TValue, TValue, Timestamp, AbstractDomainEvent<TAggregate>> createEvent)
    {
        var occurredAtUtc = Timestamp.UtcNow(clock);
        apply(next);
        _lastModified = Some(occurredAtUtc);
        AddDomainEvent(createEvent(current, next, occurredAtUtc));

        return Right<DomainError, Unit>(default);
    }

    public override string ToString() => $"{typeof(TAggregate).Name}({Id})";
}
