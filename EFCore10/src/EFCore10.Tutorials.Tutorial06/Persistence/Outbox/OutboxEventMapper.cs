using System.Text.Json;
using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Persistence.Outbox;

internal static class OutboxEventMapper
{
    private const int EventVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyDictionary<Type, Func<IDomainEvent, OutboxEventEnvelope>> Mappers =
        new Dictionary<Type, Func<IDomainEvent, OutboxEventEnvelope>>
        {
            [typeof(UserRegisteredDomainEvent)] = domainEvent => Map((UserRegisteredDomainEvent)domainEvent),
            [typeof(BlogCreatedDomainEvent)] = domainEvent => Map((BlogCreatedDomainEvent)domainEvent),
            [typeof(BlogRenamedDomainEvent)] = domainEvent => Map((BlogRenamedDomainEvent)domainEvent),
            [typeof(BlogDeletedDomainEvent)] = domainEvent => Map((BlogDeletedDomainEvent)domainEvent),
            [typeof(BlogOwnershipTransferredDomainEvent)] = domainEvent => Map((BlogOwnershipTransferredDomainEvent)domainEvent),
            [typeof(AuthorInvitedToBlogDomainEvent)] = domainEvent => Map((AuthorInvitedToBlogDomainEvent)domainEvent),
            [typeof(AuthorAcceptedBlogInvitationDomainEvent)] = domainEvent => Map((AuthorAcceptedBlogInvitationDomainEvent)domainEvent),
            [typeof(AuthorRevokedFromBlogDomainEvent)] = domainEvent => Map((AuthorRevokedFromBlogDomainEvent)domainEvent),
            [typeof(PostDraftCreatedDomainEvent)] = domainEvent => Map((PostDraftCreatedDomainEvent)domainEvent),
            [typeof(PostPublishedDomainEvent)] = domainEvent => Map((PostPublishedDomainEvent)domainEvent),
            [typeof(PostArchivedDomainEvent)] = domainEvent => Map((PostArchivedDomainEvent)domainEvent)
        };

    public static OutboxEventEnvelope Map(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return Mappers.TryGetValue(domainEvent.GetType(), out var mapper)
            ? mapper(domainEvent)
            : throw new DomainException($"Domain event '{domainEvent.GetType().Name}' does not have an outbox mapping.");
    }

    private static OutboxEventEnvelope Map(UserRegisteredDomainEvent domainEvent) =>
        Create(
            "user.registered",
            "User",
            domainEvent.UserId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                userId = domainEvent.UserId.Value
            });

    private static OutboxEventEnvelope Map(BlogCreatedDomainEvent domainEvent) =>
        Create(
            "blog.created",
            "Blog",
            domainEvent.BlogId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                blogId = domainEvent.BlogId.Value,
                ownerMembershipId = domainEvent.OwnerMembershipId.Value,
                ownerUserId = domainEvent.OwnerUserId.Value
            });

    private static OutboxEventEnvelope Map(BlogRenamedDomainEvent domainEvent) =>
        Create(
            "blog.renamed",
            "Blog",
            domainEvent.BlogId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                blogId = domainEvent.BlogId.Value,
                name = domainEvent.Name,
                renamedByUserId = domainEvent.RenamedByUserId.Value
            });

    private static OutboxEventEnvelope Map(BlogDeletedDomainEvent domainEvent) =>
        Create(
            "blog.deleted",
            "Blog",
            domainEvent.BlogId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                blogId = domainEvent.BlogId.Value,
                deletedByUserId = domainEvent.DeletedByUserId.Value
            });

    private static OutboxEventEnvelope Map(BlogOwnershipTransferredDomainEvent domainEvent) =>
        Create(
            "blog.ownership-transferred",
            "Blog",
            domainEvent.BlogId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                blogId = domainEvent.BlogId.Value,
                previousOwnerMembershipId = domainEvent.PreviousOwnerMembershipId.Value,
                newOwnerMembershipId = domainEvent.NewOwnerMembershipId.Value,
                previousOwnerUserId = domainEvent.PreviousOwnerUserId.Value,
                newOwnerUserId = domainEvent.NewOwnerUserId.Value,
                transferredByUserId = domainEvent.TransferredByUserId.Value
            });

    private static OutboxEventEnvelope Map(AuthorInvitedToBlogDomainEvent domainEvent) =>
        Create(
            "blog.author-invited",
            "Blog",
            domainEvent.BlogId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                blogId = domainEvent.BlogId.Value,
                authorMembershipId = domainEvent.AuthorMembershipId.Value,
                invitedUserId = domainEvent.InvitedUserId.Value,
                invitedByUserId = domainEvent.InvitedByUserId.Value,
                role = AuthorBlogMembershipRole.RoleKey,
                state = PendingBlogMembershipState.StateKey
            });

    private static OutboxEventEnvelope Map(AuthorAcceptedBlogInvitationDomainEvent domainEvent) =>
        Create(
            "blog.author-invitation-accepted",
            "Blog",
            domainEvent.BlogId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                blogId = domainEvent.BlogId.Value,
                authorMembershipId = domainEvent.AuthorMembershipId.Value,
                authorUserId = domainEvent.AuthorUserId.Value,
                acceptedByUserId = domainEvent.AcceptedByUserId.Value,
                role = AuthorBlogMembershipRole.RoleKey,
                state = ActiveBlogMembershipState.StateKey
            });

    private static OutboxEventEnvelope Map(AuthorRevokedFromBlogDomainEvent domainEvent) =>
        Create(
            "blog.author-revoked",
            "Blog",
            domainEvent.BlogId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                blogId = domainEvent.BlogId.Value,
                authorMembershipId = domainEvent.AuthorMembershipId.Value,
                revokedUserId = domainEvent.RevokedUserId.Value,
                revokedByUserId = domainEvent.RevokedByUserId.Value,
                role = AuthorBlogMembershipRole.RoleKey,
                state = RevokedBlogMembershipState.StateKey
            });

    private static OutboxEventEnvelope Map(PostDraftCreatedDomainEvent domainEvent) =>
        Create(
            "post.draft-created",
            "Post",
            domainEvent.PostId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                postId = domainEvent.PostId.Value,
                blogId = domainEvent.BlogId.Value,
                postedByUserId = domainEvent.PostedByUserId.Value,
                state = "Draft"
            });

    private static OutboxEventEnvelope Map(PostPublishedDomainEvent domainEvent) =>
        Create(
            "post.published",
            "Post",
            domainEvent.PostId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                postId = domainEvent.PostId.Value,
                blogId = domainEvent.BlogId.Value,
                publishedByUserId = domainEvent.PublishedByUserId.Value,
                state = "Published"
            });

    private static OutboxEventEnvelope Map(PostArchivedDomainEvent domainEvent) =>
        Create(
            "post.archived",
            "Post",
            domainEvent.PostId.ToString(),
            domainEvent.OccurredOnUtc,
            new
            {
                postId = domainEvent.PostId.Value,
                blogId = domainEvent.BlogId.Value,
                archivedByUserId = domainEvent.ArchivedByUserId.Value,
                state = "Archived"
            });

    private static OutboxEventEnvelope Create(
        string eventName,
        string aggregateType,
        string aggregateId,
        Timestamp occurredOnUtc,
        object payload) =>
        new(eventName, EventVersion, aggregateType, aggregateId, JsonSerializer.Serialize(payload, SerializerOptions), occurredOnUtc);
}
