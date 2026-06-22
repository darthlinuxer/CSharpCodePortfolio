namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostDraftCreatedDomainEvent(
    PostId PostId,
    BlogId BlogId,
    UserId PostedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
