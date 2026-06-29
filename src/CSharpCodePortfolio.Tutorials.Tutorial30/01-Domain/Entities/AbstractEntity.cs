using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Base class for entities that need identity, audit metadata, and domain events.
/// </summary>
/// <remarks>
/// <para>
/// Audit metadata is intentionally timestamp-only: <see cref="CreatedAt"/>
/// records when the aggregate came into existence, <see cref="LastModified"/>
/// records when it last changed state. Attribution ("by whom") belongs
/// at the application / authentication seam, not on the entity base —
/// this avoids coupling the domain to a concrete <c>UserAccount</c>
/// reference for actor identification (the original anti-pattern this class
/// used to carry via <c>Option&lt;UserAccount&gt; CreatedBy</c>).
/// </para>
/// <para>
/// Domain events accumulated here are cleared after the persistence
/// boundary (<see cref="RegistrationDbContext.SaveChangesAsync"/>) captures
/// them. <see cref="TimeProvider"/> is injected at the application layer
/// so tests can supply a deterministic clock (see
/// <c>Microsoft.Extensions.TimeProvider.Testing.FakeTimeProvider</c>).
/// </para>
/// </remarks>
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
    public Option<Timestamp> CreatedAt => ToOption(_createdAt);

    /// <summary>
    /// Gets the optional UTC timestamp for the latest modification.
    /// </summary>
    public Option<Timestamp> LastModified => ToOption(_lastModified);

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
}