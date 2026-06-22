namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record AuthorInvitedToBlogDomainEvent(
    BlogId BlogId,
    BlogMembershipId AuthorMembershipId,
    UserId InvitedUserId,
    UserId InvitedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
