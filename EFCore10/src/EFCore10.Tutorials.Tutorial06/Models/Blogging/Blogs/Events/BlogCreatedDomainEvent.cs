namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogCreatedDomainEvent(
    BlogId BlogId,
    BlogMembershipId OwnerMembershipId,
    UserId OwnerUserId,
    Timestamp OccurredOnUtc) : IDomainEvent
{
    public string EventName => "blog.created";

    public int EventVersion => 1;

    public string AggregateType => "Blog";

    public string AggregateId => BlogId.ToString();
}
