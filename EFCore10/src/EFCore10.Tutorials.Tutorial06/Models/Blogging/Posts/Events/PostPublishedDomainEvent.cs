namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostPublishedDomainEvent(
    PostId PostId,
    BlogId BlogId,
    UserId PublishedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent
{
    public string EventName => "post.published";

    public int EventVersion => 1;

    public string AggregateType => "Post";

    public string AggregateId => PostId.ToString();
}
