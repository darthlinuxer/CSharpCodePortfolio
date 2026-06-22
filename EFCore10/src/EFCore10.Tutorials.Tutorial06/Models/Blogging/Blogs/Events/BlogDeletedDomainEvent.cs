namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogDeletedDomainEvent(
    BlogId BlogId,
    UserId DeletedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
