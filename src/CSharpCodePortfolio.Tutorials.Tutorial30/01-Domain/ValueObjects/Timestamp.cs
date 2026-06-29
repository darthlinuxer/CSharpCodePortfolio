using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// First-class value object that guarantees domain timestamps are UTC.
/// Modeled as a <c>readonly record struct</c> so it can be compared by
/// value, accumulated without allocation, and used as a key in audit
/// sequences without boxing.
/// </summary>
/// <remarks>
/// <para>
/// Time flows from the outside: the only place that reads the clock is
/// <see cref="UtcNow(TimeProvider)"/>. Tests inject
/// <c>FakeTimeProvider</c> via <c>Microsoft.Extensions.TimeProvider.Testing</c>.
/// </para>
/// <para>
/// EF Core 10 happily materialises <c>readonly record struct</c> VOs via
/// the positional <c>Value</c> setter when a complex property is mapped
/// to one — there is no need for a private parameterless constructor.
/// </para>
/// </remarks>
public readonly record struct Timestamp(DateTime Value)
{
    /// <summary>
    /// Creates a timestamp or returns a domain error when the DateTime
    /// kind is not UTC.
    /// </summary>
    public static Either<DomainError, Timestamp> Create(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? Right<DomainError, Timestamp>(new Timestamp(value))
            : Left<DomainError, Timestamp>(new TimestampUtcRequiredError());
    }

    /// <summary>
    /// Creates a timestamp from the UTC instant exposed by the injected
    /// clock. This is the single entry point in the domain for "the
    /// current moment".
    /// </summary>
    public static Timestamp UtcNow(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return new Timestamp(clock.GetUtcNow().UtcDateTime);
    }

    /// <summary>
    /// Adds a duration while preserving the UTC timestamp invariant.
    /// </summary>
    public Timestamp Add(TimeSpan timeSpan) => new(Value.Add(timeSpan));

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
}

/// <summary>
/// Error returned when a domain timestamp is not UTC.
/// </summary>
public sealed record TimestampUtcRequiredError()
    : DomainError(new DomainErrorCode("registration.timestamp_utc_required"), "Timestamp de domínio deve estar em UTC.");