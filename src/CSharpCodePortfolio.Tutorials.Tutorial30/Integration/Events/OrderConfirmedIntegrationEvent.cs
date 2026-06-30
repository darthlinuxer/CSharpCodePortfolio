using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;

/// <summary>
/// Published Ordering contract consumed by Billing without exposing the Order aggregate.
/// </summary>
public sealed record OrderConfirmedIntegrationEvent(
    Guid Id,
    Timestamp OccurredAtUtc,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount) : IIntegrationEvent
{
    public const string EventType = "ordering.order_confirmed.v1";

    public string Type => EventType;
}
