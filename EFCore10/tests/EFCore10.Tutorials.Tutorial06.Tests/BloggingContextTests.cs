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
    public async Task SqliteRoundtripPersistsUsersBlogOwnershipAuthorInvitationPostAndOutbox()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var owner = TestDomain.CreateOwner();
        var authorUser = TestDomain.CreateAuthorUser();
        var successorOwner = TestDomain.CreateSuccessorOwner();
        var blog = TestDomain.CreateBlog(owner);
        var author = blog.InviteAuthor(authorUser);
        blog.AcceptAuthor(author.Id);
        var post = blog.CreatePost(
            authorUser,
            PostTitle.Create("Roundtrip with SQLite"),
            PostContent.Create("EF Core persists ownership roles, author invitations, and private state."));

        post.Publish();
        post.Archive();
        blog.TransferOwnership(successorOwner);

        context.AddRange(owner, authorUser, successorOwner, blog);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var persistedBlog = await context.Blogs
            .Include(savedBlog => savedBlog.Owners)
                .ThenInclude(ownerRole => ownerRole.User)
            .Include(savedBlog => savedBlog.Authors)
                .ThenInclude(authorRole => authorRole.User)
            .Include(savedBlog => savedBlog.Posts)
                .ThenInclude(savedPost => savedPost.PostedBy)
            .AsSplitQuery()
            .SingleAsync();

        var persistedPost = persistedBlog.Posts.Single();
        var persistedAuthor = persistedBlog.Authors.Single();
        var ownerHistory = persistedBlog.Owners.ToArray();
        var inactiveOwner = ownerHistory.Single(ownerRole => !ownerRole.IsActive);

        Assert.AreEqual("EF Core Notes", persistedBlog.Name.Value);
        Assert.AreEqual("https://example.com/blog", persistedBlog.Url.Value);
        Assert.HasCount(2, ownerHistory);
        Assert.AreEqual(owner.Id, inactiveOwner.UserId);
        Assert.AreEqual("mary.jackson", persistedBlog.CurrentOwner.User.UserName.Value);
        Assert.AreEqual("Accepted", persistedAuthor.StateName);
        Assert.AreEqual("Grace Hopper", persistedAuthor.User.Name.Value);
        Assert.AreEqual("Roundtrip with SQLite", persistedPost.Title.Value);
        Assert.AreEqual(authorUser.Id, persistedPost.PostedByUserId);
        Assert.AreEqual("grace.hopper", persistedPost.PostedBy.UserName.Value);
        Assert.AreEqual("Archived", persistedPost.StateName);

        var persistedOwner = await context.Users.SingleAsync(user => user.Id == owner.Id);
        StringAssert.StartsWith(persistedOwner.PasswordHash.Value, "$argon2id$v=19$m=19456,t=2,p=1$");
        Assert.IsTrue(persistedOwner.PasswordHash.VerifyPassword("Correct Horse Battery Staple 42!"));
        Assert.IsFalse(persistedOwner.PasswordHash.Value.Contains("Correct Horse Battery Staple 42!", StringComparison.Ordinal));
        Assert.AreEqual("Rua A", persistedOwner.Address.Street);
        Assert.AreEqual("SP", persistedOwner.Address.State.Value);
        Assert.AreEqual("ada@example.com", persistedOwner.Contact.Email.Value);

        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();
        CollectionAssert.IsSubsetOf(
            new[]
            {
                typeof(UserRegisteredDomainEvent).FullName,
                typeof(BlogCreatedDomainEvent).FullName,
                typeof(AuthorAcceptedBlogInvitationDomainEvent).FullName,
                typeof(BlogOwnershipTransferredDomainEvent).FullName,
                typeof(PostPublishedDomainEvent).FullName
            },
            outboxMessages.Select(message => message.Type).ToArray());
        Assert.IsTrue(outboxMessages.All(message => message.Payload.Length > 0));
        Assert.IsTrue(outboxMessages.All(message => message.Id.Version == 7));
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
    }

    [TestMethod]
    public async Task SqliteModelUsesExplicitTablesRoleEntitiesStateKeysOutboxAndDoesNotMapPersonOrDomainEvents()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var userEntityType = context.Model.FindEntityType(typeof(User));
        var blogEntityType = context.Model.FindEntityType(typeof(Blog));
        var blogOwnerEntityType = context.Model.FindEntityType(typeof(BlogOwner));
        var authorEntityType = context.Model.FindEntityType(typeof(Author));
        var postEntityType = context.Model.FindEntityType(typeof(Post));

        Assert.IsNotNull(userEntityType);
        Assert.IsNotNull(blogEntityType);
        Assert.IsNotNull(blogOwnerEntityType);
        Assert.IsNotNull(authorEntityType);
        Assert.IsNotNull(postEntityType);
        Assert.AreEqual("Users", userEntityType.GetTableName());
        Assert.AreEqual("Blogs", blogEntityType.GetTableName());
        Assert.AreEqual("BlogOwners", blogOwnerEntityType.GetTableName());
        Assert.AreEqual("Authors", authorEntityType.GetTableName());
        Assert.AreEqual("Posts", postEntityType.GetTableName());
        Assert.IsFalse(context.Model.GetEntityTypes().Any(type => type.ClrType.Name.StartsWith("Person", StringComparison.Ordinal)));
        Assert.AreEqual("UserState", userEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.AreEqual("BlogState", blogEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.AreEqual("AuthorState", authorEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.AreEqual("PostState", postEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.IsFalse(userEntityType.FindProperty(nameof(User.UserName))?.IsNullable ?? true);
        Assert.IsFalse(userEntityType.FindProperty(nameof(User.PasswordHash))?.IsNullable ?? true);
        Assert.IsFalse(userEntityType.FindProperty(nameof(User.Name))?.IsNullable ?? true);
        Assert.IsFalse(userEntityType.FindProperty(nameof(User.Document))?.IsNullable ?? true);
        Assert.IsFalse(blogEntityType.FindProperty(nameof(Blog.Name))?.IsNullable ?? true);
        Assert.IsFalse(blogEntityType.FindProperty(nameof(Blog.Url))?.IsNullable ?? true);
        Assert.IsFalse(postEntityType.FindProperty(nameof(Post.Title))?.IsNullable ?? true);
        Assert.IsFalse(postEntityType.FindProperty(nameof(Post.Content))?.IsNullable ?? true);
        Assert.IsNull(userEntityType.FindProperty("StateId"));
        Assert.IsNull(authorEntityType.FindProperty("StateId"));
        Assert.IsNull(postEntityType.FindProperty("StateId"));
        Assert.AreEqual("OutboxMessages", context.Model.FindEntityType(typeof(OutboxMessage))?.GetTableName());
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
}
