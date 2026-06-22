namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record AuthorAcceptedBlogInvitationDomainEvent(
    BlogId BlogId,
    BlogMembershipId AuthorMembershipId,
    UserId AuthorUserId,
    UserId AcceptedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
