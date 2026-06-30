using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;

/// <summary>
/// Base record for domain facts scoped to a single aggregate type.
/// </summary>
public abstract record AbstractDomainEvent<TAggregate>(string message, Timestamp OccurredAtUtc)
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public string AggregateName => typeof(TAggregate).Name;

    public sealed override string ToString() => $"{AggregateName}: {GetType().Name}";
}
