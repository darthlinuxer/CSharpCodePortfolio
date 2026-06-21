using EFCore10.Tutorials.Tutorial06.Models;
using EFCore10.Tutorials.Tutorial06.Persistence;
using EFCore10.Tutorials.Tutorial06.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore10.Tutorials.Tutorial06.Tests;

[TestClass]
public sealed class BloggingContextTests
{
    [TestMethod]
    public async Task SqliteRoundtripPersistsAuthorBlogPostValueObjectsAndState()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var author = TestDomain.CreateAuthor();
        var blog = TestDomain.CreateBlog(author);
        var post = blog.AddPost(
            PostTitle.Create("Roundtrip with SQLite"),
            PostContent.Create("EF Core persists value objects and private state."));

        post.Publish();
        post.Archive();

        context.AddRange(author, blog);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var persistedBlog = await context.Blogs
            .Include(x => x.Author)
            .Include(x => x.Posts)
            .SingleAsync();

        var persistedPost = persistedBlog.Posts.Single();

        Assert.AreEqual("Ada Lovelace", persistedBlog.Author.Name.Value);
        Assert.AreEqual("ada.lovelace", persistedBlog.Author.UserName.Value);
        Assert.AreEqual("Rua A", persistedBlog.Author.Address.Street);
        Assert.AreEqual("SP", persistedBlog.Author.Address.State.Value);
        Assert.AreEqual("ada@example.com", persistedBlog.Author.Contact.Email.Value);
        Assert.AreEqual("EF Core Notes", persistedBlog.Name.Value);
        Assert.AreEqual("https://example.com/blog", persistedBlog.Url.Value);
        Assert.AreEqual("Roundtrip with SQLite", persistedPost.Title.Value);
        Assert.AreEqual("Archived", persistedPost.StateName);

        var persistedPerson = await context.Set<Person>().SingleAsync();
        Assert.IsInstanceOfType(persistedPerson, typeof(Author));

        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();
        CollectionAssert.IsSubsetOf(
            new[]
            {
                typeof(AuthorCreatedDomainEvent).FullName,
                typeof(BlogCreatedDomainEvent).FullName,
                typeof(PostPublishedDomainEvent).FullName
            },
            outboxMessages.Select(message => message.Type).ToArray());
        Assert.IsTrue(outboxMessages.All(message => message.Payload.Length > 0));
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

        CollectionAssert.AreEquivalent(new[] { typeof(Author), typeof(Blog), typeof(Post) }, dbSetTypes);
        Assert.IsNull(typeof(BloggingContext).GetProperty("People"));
    }

    [TestMethod]
    public async Task SqliteModelUsesTphStateKeysOutboxAndDoesNotMapDomainEvents()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var authorEntityType = context.Model.FindEntityType(typeof(Author));
        var personEntityType = context.Model.FindEntityType(typeof(Person));
        var postEntityType = context.Model.FindEntityType(typeof(Post));

        Assert.IsNotNull(authorEntityType);
        Assert.IsNotNull(personEntityType);
        Assert.IsNotNull(postEntityType);
        Assert.AreEqual("People", authorEntityType.GetTableName());
        Assert.AreEqual("People", personEntityType.GetTableName());
        Assert.IsNotNull(personEntityType.FindDiscriminatorProperty());
        Assert.AreEqual("UserState", authorEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.AreEqual("PostState", postEntityType.FindProperty("StateKey")?.GetColumnName());
        Assert.IsNull(authorEntityType.FindProperty("StateId"));
        Assert.IsNull(postEntityType.FindProperty("StateId"));
        Assert.IsNull(authorEntityType.FindProperty("AuthorId"));
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
