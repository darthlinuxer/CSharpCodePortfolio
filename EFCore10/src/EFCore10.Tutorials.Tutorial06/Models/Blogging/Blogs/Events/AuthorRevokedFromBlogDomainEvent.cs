namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record AuthorRevokedFromBlogDomainEvent(
    BlogId BlogId,
    BlogMembershipId AuthorMembershipId,
    UserId RevokedUserId,
    UserId RevokedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent
{
    public string EventName => "blog.author-revoked";

    public int EventVersion => 1;

    public string AggregateType => "Blog";

    public string AggregateId => BlogId.ToString();
}
