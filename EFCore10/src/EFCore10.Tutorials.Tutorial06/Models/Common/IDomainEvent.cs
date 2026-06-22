namespace EFCore10.Tutorials.Tutorial06.Models;

public interface IDomainEvent
{
    string EventName { get; }

    int EventVersion { get; }

    string AggregateType { get; }

    string AggregateId { get; }

    Timestamp OccurredOnUtc { get; }
}
