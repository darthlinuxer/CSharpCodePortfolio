using System.Text.Json;
using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Persistence.Outbox;

public sealed class OutboxMessage
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        string eventName,
        int eventVersion,
        string aggregateType,
        string aggregateId,
        string payload,
        Timestamp occurredOnUtc)
    {
        Id = id;
        EventName = eventName;
        EventVersion = eventVersion;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; private set; }

    public string EventName { get; private set; } = string.Empty;

    public int EventVersion { get; private set; }

    public string AggregateType { get; private set; } = string.Empty;

    public string AggregateId { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public Timestamp OccurredOnUtc { get; private set; }

    public string Status { get; private set; } = "Pending";

    public int RetryCount { get; private set; }

    public Timestamp? NextAttemptOnUtc { get; private set; }

    public Timestamp? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return new OutboxMessage(
            Guid.CreateVersion7(),
            domainEvent.EventName,
            domainEvent.EventVersion,
            domainEvent.AggregateType,
            domainEvent.AggregateId,
            CreatePayload(domainEvent),
            domainEvent.OccurredOnUtc);
    }

    private static string CreatePayload(IDomainEvent domainEvent)
    {
        object payload = domainEvent switch
        {
            UserRegisteredDomainEvent domainEventItem => new
            {
                userId = domainEventItem.UserId.Value,
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            BlogCreatedDomainEvent domainEventItem => new
            {
                blogId = domainEventItem.BlogId.Value,
                ownerMembershipId = domainEventItem.OwnerMembershipId.Value,
                ownerUserId = domainEventItem.OwnerUserId.Value,
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            BlogRenamedDomainEvent domainEventItem => new
            {
                blogId = domainEventItem.BlogId.Value,
                name = domainEventItem.Name,
                renamedByUserId = domainEventItem.RenamedByUserId.Value,
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            BlogDeletedDomainEvent domainEventItem => new
            {
                blogId = domainEventItem.BlogId.Value,
                deletedByUserId = domainEventItem.DeletedByUserId.Value,
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            BlogOwnershipTransferredDomainEvent domainEventItem => new
            {
                blogId = domainEventItem.BlogId.Value,
                previousOwnerMembershipId = domainEventItem.PreviousOwnerMembershipId.Value,
                newOwnerMembershipId = domainEventItem.NewOwnerMembershipId.Value,
                previousOwnerUserId = domainEventItem.PreviousOwnerUserId.Value,
                newOwnerUserId = domainEventItem.NewOwnerUserId.Value,
                transferredByUserId = domainEventItem.TransferredByUserId.Value,
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            AuthorInvitedToBlogDomainEvent domainEventItem => new
            {
                blogId = domainEventItem.BlogId.Value,
                authorMembershipId = domainEventItem.AuthorMembershipId.Value,
                invitedUserId = domainEventItem.InvitedUserId.Value,
                invitedByUserId = domainEventItem.InvitedByUserId.Value,
                role = "Author",
                state = "Pending",
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            AuthorAcceptedBlogInvitationDomainEvent domainEventItem => new
            {
                blogId = domainEventItem.BlogId.Value,
                authorMembershipId = domainEventItem.AuthorMembershipId.Value,
                authorUserId = domainEventItem.AuthorUserId.Value,
                acceptedByUserId = domainEventItem.AcceptedByUserId.Value,
                role = "Author",
                state = "Active",
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            AuthorRevokedFromBlogDomainEvent domainEventItem => new
            {
                blogId = domainEventItem.BlogId.Value,
                authorMembershipId = domainEventItem.AuthorMembershipId.Value,
                revokedUserId = domainEventItem.RevokedUserId.Value,
                revokedByUserId = domainEventItem.RevokedByUserId.Value,
                role = "Author",
                state = "Revoked",
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            PostDraftCreatedDomainEvent domainEventItem => new
            {
                postId = domainEventItem.PostId.Value,
                blogId = domainEventItem.BlogId.Value,
                postedByUserId = domainEventItem.PostedByUserId.Value,
                state = "Draft",
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            PostPublishedDomainEvent domainEventItem => new
            {
                postId = domainEventItem.PostId.Value,
                blogId = domainEventItem.BlogId.Value,
                publishedByUserId = domainEventItem.PublishedByUserId.Value,
                state = "Published",
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            PostArchivedDomainEvent domainEventItem => new
            {
                postId = domainEventItem.PostId.Value,
                blogId = domainEventItem.BlogId.Value,
                archivedByUserId = domainEventItem.ArchivedByUserId.Value,
                state = "Archived",
                occurredOnUtc = domainEventItem.OccurredOnUtc.Value
            },
            _ => throw new DomainException($"Domain event '{domainEvent.GetType().Name}' does not have an outbox payload mapping.")
        };

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }
}
