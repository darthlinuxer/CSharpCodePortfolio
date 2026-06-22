namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostArchivedDomainEvent(
    PostId PostId,
    BlogId BlogId,
    UserId ArchivedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
