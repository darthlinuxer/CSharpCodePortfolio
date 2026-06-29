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
    public Option<Timestamp> CreatedAt => ToOption(CreatedAtValue);

    /// <summary>
    /// Gets the optional actor that created the entity.
    /// </summary>
    public Option<UserAccount> CreatedBy => ToOption(CreatedByValue);

    /// <summary>
    /// Gets the optional UTC timestamp for the latest modification.
    /// </summary>
    public Option<Timestamp> LastModified => ToOption(LastModifiedValue);

    /// <summary>
    /// Gets the optional actor that last modified the entity.
    /// </summary>
    public Option<UserAccount> LastModifiedBy => ToOption(LastModifiedByValue);

    /// <summary>
    /// Gets the internal nullable creation timestamp state that EF Core can map.
    /// </summary>
    internal Timestamp? CreatedAtValue { get; private set; }

    /// <summary>
    /// Gets the internal nullable actor state that EF Core can map as a self-reference.
    /// </summary>
    internal UserAccount? CreatedByValue { get; private set; }

    /// <summary>
    /// Gets the internal nullable modification timestamp state that EF Core can map.
    /// </summary>
    internal Timestamp? LastModifiedValue { get; private set; }

    /// <summary>
    /// Gets the internal nullable actor state that EF Core can map as a self-reference.
    /// </summary>
    internal UserAccount? LastModifiedByValue { get; private set; }

    /// <summary>
    /// Gets events raised by completed domain behavior.
    /// </summary>
    public Seq<IDomainEvent> DomainEvents => _domainEvents.ToSeq();

    /// <summary>
    /// Records creation metadata after a factory has validated the aggregate.
    /// </summary>
    protected void MarkCreated(Timestamp createdAt, Option<UserAccount> createdBy)
    {
        CreatedAtValue = createdAt;
        CreatedByValue = ToNullable(createdBy);
    }

    /// <summary>
    /// Records modification metadata after behavior changes entity state.
    /// </summary>
    protected void MarkModified(Timestamp modifiedAt, Option<UserAccount> modifiedBy)
    {
        LastModifiedValue = modifiedAt;
        LastModifiedByValue = ToNullable(modifiedBy);
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
    /// Converts nullable EF materialized state into explicit domain optionality.
    /// </summary>
    private static Option<T> ToOption<T>(T? value)
        where T : class
    {
        return value is null ? None : Some(value);
    }

    /// <summary>
    /// Converts explicit domain optionality into nullable state EF Core can map.
    /// </summary>
    private static T? ToNullable<T>(Option<T> option)
        where T : class
    {
        foreach (var value in option)
        {
            return value;
        }

        return null;
    }
}
