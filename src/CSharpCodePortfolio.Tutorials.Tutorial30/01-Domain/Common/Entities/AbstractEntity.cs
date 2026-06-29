using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Entities;

/// <summary>
/// Base class for entities that need identity, timestamp audit metadata, and domain events.
/// </summary>
/// <typeparam name="TId">Identity type used by the entity.</typeparam>
public abstract class AbstractEntity<TId> : IEntity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private Timestamp? _createdAt;
    private Timestamp? _lastModified;

    /// <summary>
    /// Initializes an empty entity for EF Core materialization.
    /// </summary>
    protected AbstractEntity()
    {
        Id = NewId();
    }

    /// <summary>
    /// Initializes the entity with its stable identity.
    /// </summary>
    protected AbstractEntity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the entity identity.
    /// </summary>
    public TId Id { get; private set; }

    /// <summary>
    /// Gets the optional UTC creation timestamp.
    /// </summary>
    public Option<Timestamp> CreatedAt => _createdAt.HasValue ? Some(_createdAt.Value) : None;

    /// <summary>
    /// Gets the optional UTC timestamp for the latest modification.
    /// </summary>
    public Option<Timestamp> LastModified => _lastModified.HasValue ? Some(_lastModified.Value) : None;

    /// <summary>
    /// Gets events raised by completed domain behavior.
    /// </summary>
    public Seq<IDomainEvent> DomainEvents => _domainEvents.ToSeq();

    /// <summary>
    /// Records creation metadata after a factory has validated the aggregate.
    /// </summary>
    protected void MarkCreated(Timestamp createdAt)
    {
        _createdAt = createdAt;
    }

    /// <summary>
    /// Records modification metadata after behavior changes entity state.
    /// </summary>
    protected void MarkModified(Timestamp modifiedAt)
    {
        _lastModified = modifiedAt;
    }

    /// <summary>
    /// Adds a domain event for effects that should happen after persistence.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears domain events after the persistence boundary has captured them.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Generates a domain identity for new Guid-backed entities; EF Core overwrites it when materializing from the database.
    /// </summary>
    private static TId NewId()
    {
        return typeof(TId) == typeof(Guid)
            ? (TId)(object)Guid.CreateVersion7()
            : default!;
    }
}
