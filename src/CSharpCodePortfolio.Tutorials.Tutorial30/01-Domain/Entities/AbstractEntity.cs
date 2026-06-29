using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Base class for entities that need identity, audit metadata, and domain events.
/// </summary>
/// <typeparam name="TId">Identity type used by the entity.</typeparam>
public abstract class AbstractEntity<TId> : IEntity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private Timestamp? _createdAt;
    private UserAccount? _createdBy;
    private Timestamp? _lastModified;
    private UserAccount? _lastModifiedBy;

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
    public Option<Timestamp> CreatedAt => ToOption(_createdAt);

    /// <summary>
    /// Gets the optional actor that created the entity.
    /// </summary>
    public Option<UserAccount> CreatedBy => ToOption(_createdBy);

    /// <summary>
    /// Gets the optional UTC timestamp for the latest modification.
    /// </summary>
    public Option<Timestamp> LastModified => ToOption(_lastModified);

    /// <summary>
    /// Gets the optional actor that last modified the entity.
    /// </summary>
    public Option<UserAccount> LastModifiedBy => ToOption(_lastModifiedBy);

    /// <summary>
    /// Gets events raised by completed domain behavior.
    /// </summary>
    public Seq<IDomainEvent> DomainEvents => _domainEvents.ToSeq();

    /// <summary>
    /// Records creation metadata after a factory has validated the aggregate.
    /// </summary>
    protected void MarkCreated(Timestamp createdAt, Option<UserAccount> createdBy)
    {
        _createdAt = createdAt;
        _createdBy = ToNullable(createdBy);
    }

    /// <summary>
    /// Records modification metadata after behavior changes entity state.
    /// </summary>
    protected void MarkModified(Timestamp modifiedAt, Option<UserAccount> modifiedBy)
    {
        _lastModified = modifiedAt;
        _lastModifiedBy = ToNullable(modifiedBy);
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

    /// <summary>
    /// Converts a nullable value-type EF-materialized state into an explicit
    /// domain Option. Constraint is <c>struct</c> so this helper is compatible
    /// with the readonly record struct value objects used by the domain.
    /// </summary>
    private static Option<T> ToOption<T>(T? value)
        where T : struct
    {
        return value.HasValue ? Some(value.Value) : None;
    }

    /// <summary>
    /// Converts an explicit domain Option into a nullable value-type that EF
    /// Core can materialise. Constraint is <c>struct</c> so the helper pairs
    /// with <see cref="ToOption{T}(T?)"/>.
    /// </summary>
    private static T? ToNullable<T>(Option<T> option)
        where T : struct
    {
        foreach (var value in option)
        {
            return value;
        }

        return null;
    }
}
