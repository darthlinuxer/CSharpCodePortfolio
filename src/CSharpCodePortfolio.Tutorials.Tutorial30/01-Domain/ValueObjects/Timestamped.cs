using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// First-class wrapper around a value plus its timestamp. Used at the
/// application/aggregate boundary so that audit timestamps travel with
/// the value they describe without forcing the aggregate to read the
/// ambient clock.
/// </summary>
/// <remarks>
/// Equality is structural across <c>Value</c> and <c>At</c> (struct value
/// semantics). Allocation-free per instance.
/// </remarks>
/// <typeparam name="T">Wrapped value type (typically a domain VO).</typeparam>
public readonly record struct Timestamped<T>(T Value, Timestamp At)
    where T : struct
{
    /// <summary>
    /// Wraps a value with its timestamp.
    /// </summary>
    public static Either<Seq<DomainError>, Timestamped<T>> Create(T value, Timestamp at)
    {
        return Right<Seq<DomainError>, Timestamped<T>>(new Timestamped<T>(value, at));
    }

    /// <summary>
    /// Wraps a value with a freshly-read UTC instant from the supplied clock.
    /// </summary>
    public static Timestamped<T> Now(T value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return new Timestamped<T>(value, Timestamp.UtcNow(clock));
    }
}