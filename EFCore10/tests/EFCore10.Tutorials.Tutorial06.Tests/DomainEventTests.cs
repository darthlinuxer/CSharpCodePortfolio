using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Tests;

[TestClass]
public sealed class DomainEventTests
{
    [TestMethod]
    public void AuthorCreateRaisesAuthorCreatedDomainEvent()
    {
        var author = TestDomain.CreateAuthor();

        var domainEvent = AssertHasSingleEvent<AuthorCreatedDomainEvent>(author);
        Assert.AreEqual(author.AuthorId, domainEvent.AuthorId);
    }

    [TestMethod]
    public void BlogCreateRaisesBlogCreatedDomainEvent()
    {
        var author = TestDomain.CreateAuthor();
        var blog = TestDomain.CreateBlog(author);

        var domainEvent = AssertHasSingleEvent<BlogCreatedDomainEvent>(blog);
        Assert.AreEqual(blog.Id, domainEvent.BlogId);
        Assert.AreEqual(author.Id, domainEvent.AuthorId);
    }

    [TestMethod]
    public void BlogAddPostRaisesBlogAndPostDomainEvents()
    {
        var blog = TestDomain.CreateBlog(TestDomain.CreateAuthor());

        var post = blog.AddPost(
            PostTitle.Create("Domain events in EF"),
            PostContent.Create("Domain events stay in memory."));

        var addedEvent = AssertHasSingleEvent<PostAddedToBlogDomainEvent>(blog);
        var createdEvent = AssertHasSingleEvent<PostCreatedDomainEvent>(post);

        Assert.AreEqual(blog.Id, addedEvent.BlogId);
        Assert.AreEqual(post.Id, addedEvent.PostId);
        Assert.AreEqual(post.Id, createdEvent.PostId);
    }

    [TestMethod]
    public void ClearDomainEventsEmptiesInMemoryEvents()
    {
        var author = TestDomain.CreateAuthor();

        author.ClearDomainEvents();

        Assert.IsFalse(author.DomainEvents.Any());
    }

    private static TEvent AssertHasSingleEvent<TEvent>(AggregateRoot aggregate)
        where TEvent : IDomainEvent
    {
        var domainEvent = aggregate.DomainEvents.OfType<TEvent>().Single();
        Assert.IsInstanceOfType(domainEvent, typeof(TEvent));

        return domainEvent;
    }
}
