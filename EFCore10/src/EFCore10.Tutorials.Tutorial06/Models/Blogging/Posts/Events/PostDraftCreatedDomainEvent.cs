namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostDraftCreatedDomainEvent(
    PostId PostId,
    BlogId BlogId,
    UserId PostedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent
{
    public string EventName => "post.draft-created";

    public int EventVersion => 1;

    public string AggregateType => "Post";

    public string AggregateId => PostId.ToString();
}
