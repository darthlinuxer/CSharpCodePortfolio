namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogCreatedDomainEvent(
    BlogId BlogId,
    BlogMembershipId OwnerMembershipId,
    UserId OwnerUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
