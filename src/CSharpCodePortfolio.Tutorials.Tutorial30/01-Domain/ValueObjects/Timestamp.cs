using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Value object that guarantees domain timestamps are UTC.
/// </summary>
/// <remarks>
/// Kept as a <c>sealed record class</c> (with <c>private init</c>-only
/// setters) in Task 0 because <see cref="AbstractEntity{TId}"/> still calls
/// <c>ToOption&lt;T&gt;(T? value) where T : class</c> for the
/// <c>_createdAt</c> / <c>_lastModified</c> fields. Task 1 will convert
/// all value objects to <c>readonly record struct</c> and lift
/// <c>AbstractEntity</c> at the same time. The <c>private init</c> form
/// already removes the EF-only parameterless constructor hack that the
/// old <c>private set</c> form carried.
/// </remarks>
public sealed record Timestamp
{
    /// <summary>
    /// Initializes an empty timestamp for EF Core materialization.
    /// </summary>
    private Timestamp()
    {
    }

    /// <summary>
    /// Initializes a timestamp with the supplied UTC value.
    /// </summary>
    private Timestamp(DateTime value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the UTC DateTime value.
    /// </summary>
    public DateTime Value { get; private init; }

    /// <summary>
    /// Creates a timestamp or returns a domain error when the DateTime kind is not UTC.
    /// </summary>
    public static Either<DomainError, Timestamp> Create(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? Right<DomainError, Timestamp>(new Timestamp(value))
            : Left<DomainError, Timestamp>(new TimestampUtcRequiredError());
    }

    /// <summary>
    /// Creates a timestamp from the UTC instant exposed by the injected clock.
    /// </summary>
    /// <remarks>
    /// This is the single entry point in the domain for "the current moment".
    /// All aggregate methods accept a <see cref="TimeProvider"/> parameter and
    /// route through here so tests can substitute a deterministic clock.
    /// </remarks>
    public static Timestamp UtcNow(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return FromClock(clock);
    }

    /// <summary>
    /// Adds a duration while preserving the UTC timestamp invariant.
    /// </summary>
    public Timestamp Add(TimeSpan timeSpan) =>
        new(Value.Add(timeSpan));

    /// <summary>
    /// Compares two timestamps by their UTC value.
    /// </summary>
    public static bool operator >(Timestamp left, Timestamp right) => left.Value > right.Value;

    /// <summary>
    /// Compares two timestamps by their UTC value.
    /// </summary>
    public static bool operator >=(Timestamp left, Timestamp right) => left.Value >= right.Value;

    /// <summary>
    /// Compares two timestamps by their UTC value.
    /// </summary>
    public static bool operator <(Timestamp left, Timestamp right) => left.Value < right.Value;

    /// <summary>
    /// Compares two timestamps by their UTC value.
    /// </summary>
    public static bool operator <=(Timestamp left, Timestamp right) => left.Value <= right.Value;

    /// <summary>
    /// Converts a <see cref="TimeProvider"/> instant into a domain <see cref="Timestamp"/>.
    /// </summary>
    private static Timestamp FromClock(TimeProvider clock) =>
        new(clock.GetUtcNow().UtcDateTime);
}

/// <summary>
/// Error returned when a domain timestamp is not UTC.
/// </summary>
public sealed record TimestampUtcRequiredError()
    : DomainError(new DomainErrorCode("registration.timestamp_utc_required"), "Timestamp de domínio deve estar em UTC.");

