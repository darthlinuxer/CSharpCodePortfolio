namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogOwnershipTransferredDomainEvent(
    BlogId BlogId,
    BlogMembershipId PreviousOwnerMembershipId,
    BlogMembershipId NewOwnerMembershipId,
    UserId PreviousOwnerUserId,
    UserId NewOwnerUserId,
    UserId TransferredByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
