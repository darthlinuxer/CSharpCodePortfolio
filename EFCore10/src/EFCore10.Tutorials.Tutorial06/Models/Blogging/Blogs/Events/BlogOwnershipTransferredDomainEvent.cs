namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogOwnershipTransferredDomainEvent(
    BlogId BlogId,
    BlogMembershipId PreviousOwnerMembershipId,
    BlogMembershipId NewOwnerMembershipId,
    UserId PreviousOwnerUserId,
    UserId NewOwnerUserId,
    UserId TransferredByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent
{
    public string EventName => "blog.ownership-transferred";

    public int EventVersion => 1;

    public string AggregateType => "Blog";

    public string AggregateId => BlogId.ToString();
}
