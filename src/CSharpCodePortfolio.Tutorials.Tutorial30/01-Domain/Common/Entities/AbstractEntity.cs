using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Entities;

/// <summary>
/// Base class for entities that need identity, timestamp audit metadata, and domain events.
/// </summary>
/// <typeparam name="TAggregate">Aggregate type allowed to raise events through this entity.</typeparam>
/// <typeparam name="TId">Identity type used by the entity.</typeparam>
public abstract class AbstractEntity<TAggregate, TId> : IEntity<TAggregate, TId>
    where TId : notnull
{
    private readonly List<AbstractDomainEvent<TAggregate>> _domainEvents = [];
    private Option<Timestamp> _createdAt = None;
    private Option<Timestamp> _lastModified = None;

    /// <summary>
    /// Initializes an empty entity for EF Core materialization.
    /// </summary>
    protected AbstractEntity() => Id = NewId();

    /// <summary>
    /// Initializes the entity with its stable identity.
    /// </summary>
    protected AbstractEntity(TId id) => Id = id;

    /// <summary>
    /// Gets the entity identity.
    /// </summary>
    public TId Id { get; private set; }

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

        if (EqualityComparer<TValue>.Default.Equals(current, next))
        {
            return Right<DomainError, Unit>(default);
        }

        var occurredAtUtc = Timestamp.UtcNow(clock);
        apply(next);
        _lastModified = Some(occurredAtUtc);
        AddDomainEvent(createEvent(current, next, occurredAtUtc));

        return Right<DomainError, Unit>(default);
    }

    /// <summary>
    /// Adds a domain event for effects that should happen after persistence.
    /// </summary>
    private void AddDomainEvent(AbstractDomainEvent<TAggregate> domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears domain events after the persistence boundary has captured them.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    public override string ToString() => $"{typeof(TAggregate).Name}({Id})";

    /// <summary>
    /// Generates a domain identity for new Guid-backed entities; EF Core overwrites it when materializing from the database.
    /// </summary>
    private static TId NewId()
    {
        if (typeof(TId) == typeof(Guid))
        {
            return (TId)(object)Guid.CreateVersion7();
        }

        throw new NotSupportedException($"Automatic identity generation is not configured for {typeof(TId).Name}.");
    }
}
