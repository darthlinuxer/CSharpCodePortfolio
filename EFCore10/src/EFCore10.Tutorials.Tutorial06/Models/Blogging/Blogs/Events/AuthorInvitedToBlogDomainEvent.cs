namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record AuthorInvitedToBlogDomainEvent(
    BlogId BlogId,
    AuthorId AuthorId,
    UserId InvitedUserId,
    UserId InvitedByUserId,
    DateTime OccurredOnUtc) : IDomainEvent;
