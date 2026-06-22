namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record AuthorRevokedFromBlogDomainEvent(
    BlogId BlogId,
    BlogMembershipId AuthorMembershipId,
    UserId RevokedUserId,
    UserId RevokedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
