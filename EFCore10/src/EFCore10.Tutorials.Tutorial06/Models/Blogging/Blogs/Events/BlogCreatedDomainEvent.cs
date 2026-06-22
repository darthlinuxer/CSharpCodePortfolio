namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogCreatedDomainEvent(
    BlogId BlogId,
    BlogOwnerId BlogOwnerId,
    UserId OwnerUserId,
    DateTime OccurredOnUtc) : IDomainEvent;
