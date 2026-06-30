using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;

/// <summary>
/// Published-language event that may cross a bounded-context boundary.
/// </summary>
public interface IIntegrationEvent
{
    Guid Id { get; }
    Timestamp OccurredAtUtc { get; }
    string Type { get; }
}
