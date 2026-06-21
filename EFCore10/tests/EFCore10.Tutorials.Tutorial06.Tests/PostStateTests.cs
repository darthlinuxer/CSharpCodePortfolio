using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Tests;

[TestClass]
public sealed class PostStateTests
{
    [TestMethod]
    public void PublishMovesDraftPostToPublishedAndRaisesEvent()
    {
        var post = TestDomain.CreatePost();
        post.ClearDomainEvents();

        post.Publish();

        Assert.AreEqual("Published", post.StateName);
        var domainEvent = (PostPublishedDomainEvent)post.DomainEvents.Single();
        Assert.IsInstanceOfType(domainEvent, typeof(PostPublishedDomainEvent));
        Assert.AreEqual(post.Id, domainEvent.PostId);
    }

    [TestMethod]
    public void PublishFailsWhenPostIsArchived()
    {
        var post = TestDomain.CreatePost();
        post.Publish();
        post.Archive();

        var exception = Assert.ThrowsExactly<DomainException>(post.Publish);

        Assert.AreEqual("Archived posts cannot be published.", exception.Message);
    }

    [TestMethod]
    public void ArchiveMovesPublishedPostToArchivedAndRaisesEvent()
    {
        var post = TestDomain.CreatePost();
        post.Publish();
        post.ClearDomainEvents();

        post.Archive();

        Assert.AreEqual("Archived", post.StateName);
        var domainEvent = (PostArchivedDomainEvent)post.DomainEvents.Single();
        Assert.IsInstanceOfType(domainEvent, typeof(PostArchivedDomainEvent));
        Assert.AreEqual(post.Id, domainEvent.PostId);
    }
}
