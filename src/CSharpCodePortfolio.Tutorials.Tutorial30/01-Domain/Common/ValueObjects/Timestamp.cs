using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;

/// <summary>
/// Value object that guarantees domain timestamps are UTC.
/// </summary>
public readonly record struct Timestamp(DateTime Value)
{
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
    /// Creates a timestamp from the current UTC clock value.
    /// </summary>
    public static Timestamp UtcNow(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return new Timestamp(clock.GetUtcNow().UtcDateTime);
    }

    /// <summary>
    /// Rehydrates a UTC timestamp read from a trusted storage boundary.
    /// </summary>
    internal static Timestamp FromTrustedUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

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
}

/// <summary>
/// Error returned when a domain timestamp is not UTC.
/// </summary>
public sealed record TimestampUtcRequiredError()
    : DomainError(new DomainErrorCode("registration.timestamp_utc_required"), "Timestamp de domínio deve estar em UTC.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
