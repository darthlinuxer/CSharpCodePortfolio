using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;

/// <summary>
/// Published Identity contract consumed by Ordering as a local customer reference.
/// </summary>
public sealed record UserRegisteredIntegrationEvent(
    Guid Id,
    Timestamp OccurredAtUtc,
    Guid UserAccountId,
    string Email) : IIntegrationEvent
{
    public const string EventType = "identity.user_registered.v1";

    public string Type => EventType;
}
