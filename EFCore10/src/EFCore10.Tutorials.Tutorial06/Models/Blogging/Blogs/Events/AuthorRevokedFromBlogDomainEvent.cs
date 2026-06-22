namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record AuthorRevokedFromBlogDomainEvent(
    BlogId BlogId,
    AuthorId AuthorId,
    UserId RevokedUserId,
    UserId RevokedByUserId,
    DateTime OccurredOnUtc) : IDomainEvent;
