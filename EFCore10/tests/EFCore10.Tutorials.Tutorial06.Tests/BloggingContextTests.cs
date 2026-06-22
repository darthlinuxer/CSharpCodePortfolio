using System.Text.Json;
using EFCore10.Tutorials.Tutorial06.Models;
using EFCore10.Tutorials.Tutorial06.Persistence;
using EFCore10.Tutorials.Tutorial06.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial06.Tests;

[TestClass]
public sealed class BloggingContextTests
{
    [TestMethod]
    public async Task SqliteRoundtripPersistsUsersBlogMembershipsPostTimestampsAndOutboxEnvelope()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var owner = TestDomain.CreateOwner();
        var authorUser = TestDomain.CreateAuthorUser();
        var successorOwner = TestDomain.CreateSuccessorOwner();
        var blog = TestDomain.CreateBlog(owner);
        var authorMembership = blog.InviteAuthor(authorUser, owner);
        blog.AcceptAuthorInvitation(authorMembership.Id, authorUser);
        var post = blog.CreatePost(
            authorUser,
            PostTitle.Create("Roundtrip with SQLite"),
            PostContent.Create("EF Core persists memberships, post timestamps, and outbox envelopes."));

        post.Publish(authorUser.Id);
        post.Archive(owner.Id);
        blog.TransferOwnership(successorOwner, owner);

        context.AddRange(owner, authorUser, successorOwner, blog);
        await context.SaveChangesAsync();

        Assert.IsTrue(new AggregateRoot[] { owner, authorUser, successorOwner, blog, post }
            .All(aggregate => aggregate.DomainEvents.Count == 0));

        context.ChangeTracker.Clear();

        var persistedBlog = await context.Blogs
            .Include(savedBlog => savedBlog.Memberships)
                .ThenInclude(membership => membership.User)
            .Include(savedBlog => savedBlog.Posts)
                .ThenInclude(savedPost => savedPost.PostedBy)
            .AsSplitQuery()
            .SingleAsync();

        var persistedPost = persistedBlog.Posts.Single();
        var memberships = persistedBlog.Memberships.ToArray();
        var activeAuthor = persistedBlog.Authors.Single();
        var inactiveOwner = memberships.Single(membership => membership.RoleName == "Owner" && !membership.IsActive);

        Assert.AreEqual("EF Core Notes", persistedBlog.Name.Value);
        Assert.AreEqual("https://example.com/blog", persistedBlog.Url.Value);
        Assert.HasCount(3, memberships);
        Assert.AreEqual(owner.Id, inactiveOwner.UserId);
        Assert.AreEqual("Ended", inactiveOwner.StateName);
        Assert.AreEqual("mary.jackson", persistedBlog.CurrentOwner.User.UserName.Value);
        Assert.AreEqual("Author", activeAuthor.RoleName);
        Assert.AreEqual("Active", activeAuthor.StateName);
        Assert.AreEqual("Grace Hopper", activeAuthor.User.Name.Value);
        Assert.AreEqual("Roundtrip with SQLite", persistedPost.Title.Value);
        Assert.AreEqual(authorUser.Id, persistedPost.PostedByUserId);
        Assert.AreEqual("grace.hopper", persistedPost.PostedBy.UserName.Value);
        Assert.AreEqual("Archived", persistedPost.StateName);
        Assert.AreEqual(DateTimeKind.Utc, persistedPost.CreatedOnUtc.Value.Kind);
        Assert.AreEqual(DateTimeKind.Utc, persistedPost.PublishedOnUtc?.Value.Kind);
        Assert.AreEqual(DateTimeKind.Utc, persistedPost.ArchivedOnUtc?.Value.Kind);

        var persistedOwner = await context.Users.SingleAsync(user => user.Id == owner.Id);
        StringAssert.StartsWith(persistedOwner.PasswordHash.Value, "$argon2id$v=19$m=19456,t=2,p=1$");
        Assert.IsTrue(persistedOwner.PasswordHash.VerifyPassword("Correct Horse Battery Staple 42!"));
        Assert.IsFalse(persistedOwner.PasswordHash.Value.Contains("Correct Horse Battery Staple 42!", StringComparison.Ordinal));
        Assert.AreEqual("Rua A", persistedOwner.Address.Street);
        Assert.AreEqual("SP", persistedOwner.Address.State.Value);
        Assert.AreEqual("ada@example.com", persistedOwner.Contact.Email.Value);

        var publishedOnUtc = persistedPost.PublishedOnUtc.GetValueOrDefault();
        var publishedFrom = publishedOnUtc.Add(TimeSpan.FromMinutes(-1));
        var publishedTo = publishedOnUtc.Add(TimeSpan.FromMinutes(1));
        var postsInRange = await context.Posts
            .AsNoTracking()
            .Where(savedPost => savedPost.BlogId == persistedBlog.Id)
            .Where(savedPost => savedPost.PublishedOnUtc >= publishedFrom && savedPost.PublishedOnUtc <= publishedTo)
            .ToListAsync();

        Assert.HasCount(1, postsInRange);
        Assert.AreEqual(persistedPost.Id, postsInRange.Single().Id);

        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();
        CollectionAssert.IsSubsetOf(
            new[]
            {
                "user.registered",
                "blog.created",
                "blog.author-invited",
                "blog.author-invitation-accepted",
                "blog.ownership-transferred",
                "post.published"
            },
            outboxMessages.Select(message => message.EventName).ToArray());
        Assert.IsTrue(outboxMessages.All(message => message.EventVersion == 1));
        Assert.IsTrue(outboxMessages.All(message => message.Id.Version == 7));
        Assert.IsTrue(outboxMessages.All(message => message.AggregateId.Length > 0));
        Assert.IsTrue(outboxMessages.All(message => message.Status == "Pending"));
        Assert.IsTrue(outboxMessages.All(message => message.RetryCount == 0));
        Assert.IsTrue(outboxMessages.All(message => message.Payload.Length > 0));

        using var payload = JsonDocument.Parse(outboxMessages.Single(message => message.EventName == "blog.created").Payload);
        Assert.IsTrue(payload.RootElement.TryGetProperty("blogId", out _));
        Assert.IsTrue(payload.RootElement.TryGetProperty("ownerMembershipId", out _));
        Assert.IsFalse(payload.RootElement.TryGetProperty("occurredOnUtc", out _));
    }

    [TestMethod]
    public void OutboxMapperRejectsUnmappedDomainEvents()
    {
        var domainEvent = new UnmappedDomainEvent(Timestamp.UtcNow);

        var exception = Assert.ThrowsExactly<DomainException>(() => OutboxMessage.FromDomainEvent(domainEvent));

        Assert.AreEqual("Domain event 'UnmappedDomainEvent' does not have an outbox mapping.", exception.Message);
    }

    [TestMethod]
    public void BloggingContextExposesOnlyConcreteAggregateSets()
    {
        var dbSetTypes = typeof(BloggingContext)
            .GetProperties()
            .Where(property => property.PropertyType.IsGenericType)
            .Where(property => property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(property => property.PropertyType.GetGenericArguments()[0])
            .ToArray();

        CollectionAssert.AreEquivalent(new[] { typeof(User), typeof(Blog), typeof(Post) }, dbSetTypes);
        Assert.IsNull(typeof(BloggingContext).GetProperty("People"));
        Assert.IsNull(typeof(BloggingContext).GetProperty("Authors"));
        Assert.IsNull(typeof(BloggingContext).GetProperty("BlogOwners"));
        Assert.IsNull(typeof(BloggingContext).GetProperty("BlogMemberships"));
    }

    [TestMethod]
    public async Task SqliteModelUsesMembershipTableTimestampColumnsOutboxEnvelopeAndDoesNotMapDomainEvents()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var userEntityType = context.Model.FindEntityType(typeof(User));
        var blogEntityType = context.Model.FindEntityType(typeof(Blog));
        var membershipEntityType = context.Model.FindEntityType(typeof(BlogMembership));
        var postEntityType = context.Model.FindEntityType(typeof(Post));
        var outboxEntityType = context.Model.FindEntityType(typeof(OutboxMessage));
        var tableNames = context.Model.GetEntityTypes().Select(type => type.GetTableName()).ToArray();

        Assert.IsNotNull(userEntityType);
        Assert.IsNotNull(blogEntityType);
        Assert.IsNotNull(membershipEntityType);
        Assert.IsNotNull(postEntityType);
        Assert.IsNotNull(outboxEntityType);
        Assert.AreEqual("Users", userEntityType.GetTableName());
        Assert.AreEqual("Blogs", blogEntityType.GetTableName());
        Assert.AreEqual("BlogMemberships", membershipEntityType.GetTableName());
        Assert.AreEqual("Posts", postEntityType.GetTableName());
        Assert.AreEqual("OutboxMessages", outboxEntityType.GetTableName());
        CollectionAssert.DoesNotContain(tableNames, "Authors");
        CollectionAssert.DoesNotContain(tableNames, "BlogOwners");
        Assert.IsFalse(context.Model.GetEntityTypes().Any(type => type.ClrType.Name.StartsWith("Person", StringComparison.Ordinal)));
        Assert.AreEqual("UserState", userEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.AreEqual("BlogState", blogEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.AreEqual("MembershipRole", membershipEntityType.FindProperty("RoleKey")?.GetColumnName());
        Assert.AreEqual("MembershipState", membershipEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.AreEqual("PostState", postEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.IsNotNull(postEntityType.FindProperty(nameof(Post.CreatedOnUtc)));
        Assert.IsNotNull(postEntityType.FindProperty(nameof(Post.PublishedOnUtc)));
        Assert.IsNotNull(postEntityType.FindProperty(nameof(Post.ArchivedOnUtc)));
        Assert.IsNotNull(outboxEntityType.FindProperty(nameof(OutboxMessage.EventName)));
        Assert.IsNotNull(outboxEntityType.FindProperty(nameof(OutboxMessage.EventVersion)));
        Assert.IsNotNull(outboxEntityType.FindProperty(nameof(OutboxMessage.AggregateType)));
        Assert.IsNotNull(outboxEntityType.FindProperty(nameof(OutboxMessage.AggregateId)));
        Assert.IsNotNull(outboxEntityType.FindProperty(nameof(OutboxMessage.Status)));
        Assert.IsNull(outboxEntityType.FindProperty("Type"));
        Assert.IsFalse(context.Model.GetEntityTypes().Any(type => typeof(IDomainEvent).IsAssignableFrom(type.ClrType)));
        Assert.IsTrue(context.Model.GetEntityTypes().All(type => !type.GetProperties().Any(property => property.Name == nameof(AggregateRoot.DomainEvents))));
    }

    private static BloggingContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<BloggingContext>()
            .UseSqlite(connection)
            .Options;

        return new BloggingContext(options);
    }

    private sealed record UnmappedDomainEvent(Timestamp OccurredOnUtc) : IDomainEvent;
}
