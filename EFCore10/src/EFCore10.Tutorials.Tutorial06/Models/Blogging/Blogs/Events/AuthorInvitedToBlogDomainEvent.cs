namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record AuthorInvitedToBlogDomainEvent(
    BlogId BlogId,
    BlogMembershipId AuthorMembershipId,
    UserId InvitedUserId,
    UserId InvitedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent
{
    public string EventName => "blog.author-invited";

    public int EventVersion => 1;

    public string AggregateType => "Blog";

    public string AggregateId => BlogId.ToString();
}
