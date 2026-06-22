namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record UserRegisteredDomainEvent(
    UserId UserId,
    Timestamp OccurredOnUtc) : IDomainEvent
{
    public string EventName => "user.registered";

    public int EventVersion => 1;

    public string AggregateType => "User";

    public string AggregateId => UserId.ToString();
}
