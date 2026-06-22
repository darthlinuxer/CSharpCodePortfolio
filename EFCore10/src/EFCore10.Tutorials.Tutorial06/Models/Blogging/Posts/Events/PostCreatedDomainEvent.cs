namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostCreatedDomainEvent(
    PostId PostId,
    BlogId BlogId,
    UserId PostedByUserId,
    DateTime OccurredOnUtc) : IDomainEvent;
