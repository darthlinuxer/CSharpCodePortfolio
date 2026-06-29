using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.Extensions.Time.Testing;
using System.Reflection;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Verifies that <see cref="Timestamp.UtcNow"/> reads the injected <see cref="TimeProvider"/>
/// instead of the ambient <see cref="DateTime.UtcNow"/> clock, so domain time is
/// deterministic in tests.
/// </summary>
[TestClass]
public sealed class TimestampProviderTests
{
    /// <summary>
    /// Proves that <see cref="Timestamp.UtcNow(TimeProvider)"/> returns the exact
    /// UTC instant exposed by the injected clock.
    /// </summary>
    [TestMethod]
    public void Timestamp_UtcNow_UsesTimeProviderUtcNow()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero));

        var timestamp = Timestamp.UtcNow(clock);

        Assert.AreEqual(clock.GetUtcNow().UtcDateTime, timestamp.Value);
        Assert.AreEqual(DateTimeKind.Utc, timestamp.Value.Kind);
    }

    /// <summary>
    /// Proves that advancing the injected clock is observed by the next
    /// <see cref="Timestamp.UtcNow(TimeProvider)"/> call, which is the whole
    /// reason for replacing <c>DateTime.UtcNow</c> with an injected clock.
    /// </summary>
    [TestMethod]
    public void Timestamp_UtcNow_ChangesWhenClockAdvances()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero));

        var before = Timestamp.UtcNow(clock);
        clock.Advance(TimeSpan.FromHours(1));
        var after = Timestamp.UtcNow(clock);

        Assert.AreEqual(clock.GetUtcNow().UtcDateTime, after.Value);
        Assert.AreEqual(before.Value.AddHours(1), after.Value);
    }

    /// <summary>
    /// Proves that the previous parameterless factory shape is gone: every
    /// <see cref="Timestamp.UtcNow(TimeProvider)"/> call must source the
    /// instant from the injected clock, never from the system clock.
    /// </summary>
    [TestMethod]
    public void Timestamp_UtcNow_HasNoParameterlessOverload()
    {
        var parameterless = typeof(Timestamp)
            .GetMethods()
            .Where(method => method.Name == nameof(Timestamp.UtcNow))
            .Where(method => method.GetParameters().Length == 0)
            .ToArray();

        var propertyAccessors = typeof(Timestamp)
            .GetProperties()
            .Where(property => property.Name == nameof(Timestamp.UtcNow))
            .ToArray();

        Assert.IsEmpty(parameterless);
        Assert.IsEmpty(propertyAccessors);
    }
}