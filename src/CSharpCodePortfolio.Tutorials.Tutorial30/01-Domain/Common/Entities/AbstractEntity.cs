using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Functional;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Entities;

/// <summary>
/// Base class for entities that need identity, audit metadata, and domain events.
/// </summary>
/// <typeparam name="TId">Identity type used by the entity.</typeparam>
/// <typeparam name="TActor">Actor aggregate type used by audit metadata.</typeparam>
public abstract class AbstractEntity<TId, TActor> : IEntity<TId, TActor>
    where TId : notnull
    where TActor : class
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private Timestamp? _createdAt;
    private TActor? _createdBy;
    private Timestamp? _lastModified;
    private TActor? _lastModifiedBy;

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
    public Option<Timestamp> CreatedAt => _createdAt.ToOption();

    /// <summary>
    /// Gets the optional actor that created the entity.
    /// </summary>
    public Option<TActor> CreatedBy => _createdBy.ToOption();

    /// <summary>
    /// Gets the optional UTC timestamp for the latest modification.
    /// </summary>
    public Option<Timestamp> LastModified => _lastModified.ToOption();

    /// <summary>
    /// Gets the optional actor that last modified the entity.
    /// </summary>
    public Option<TActor> LastModifiedBy => _lastModifiedBy.ToOption();

    /// <summary>
    /// Gets events raised by completed domain behavior.
    /// </summary>
    public Seq<IDomainEvent> DomainEvents => _domainEvents.ToSeq();

    /// <summary>
    /// Records creation metadata after a factory has validated the aggregate.
    /// </summary>
    protected void MarkCreated(Timestamp createdAt, Option<TActor> createdBy)
    {
        _createdAt = createdAt;
        _createdBy = createdBy.ToNullable();
    }

    /// <summary>
    /// Records modification metadata after behavior changes entity state.
    /// </summary>
    protected void MarkModified(Timestamp modifiedAt, Option<TActor> modifiedBy)
    {
        _lastModified = modifiedAt;
        _lastModifiedBy = modifiedBy.ToNullable();
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
