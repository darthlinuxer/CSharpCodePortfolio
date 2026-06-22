namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogOwnershipTransferredDomainEvent(
    BlogId BlogId,
    BlogOwnerId PreviousBlogOwnerId,
    BlogOwnerId NewBlogOwnerId,
    UserId PreviousOwnerUserId,
    UserId NewOwnerUserId,
    DateTime OccurredOnUtc) : IDomainEvent;
