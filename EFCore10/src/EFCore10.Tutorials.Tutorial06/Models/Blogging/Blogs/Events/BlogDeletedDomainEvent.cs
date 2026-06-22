namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogDeletedDomainEvent(
    BlogId BlogId,
    UserId DeletedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent
{
    public string EventName => "blog.deleted";

    public int EventVersion => 1;

    public string AggregateType => "Blog";

    public string AggregateId => BlogId.ToString();
}
