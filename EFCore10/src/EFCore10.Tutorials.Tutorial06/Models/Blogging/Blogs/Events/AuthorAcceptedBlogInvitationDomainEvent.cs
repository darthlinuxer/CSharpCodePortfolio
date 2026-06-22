namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record AuthorAcceptedBlogInvitationDomainEvent(
    BlogId BlogId,
    AuthorId AuthorId,
    UserId AuthorUserId,
    DateTime OccurredOnUtc) : IDomainEvent;
