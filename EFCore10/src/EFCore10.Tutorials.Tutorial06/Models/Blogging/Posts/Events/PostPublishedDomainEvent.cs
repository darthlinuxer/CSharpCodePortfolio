namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostPublishedDomainEvent(
    PostId PostId,
    BlogId BlogId,
    UserId PublishedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
