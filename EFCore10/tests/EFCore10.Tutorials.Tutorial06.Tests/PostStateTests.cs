using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Tests;

[TestClass]
public sealed class PostStateTests
{
    [TestMethod]
    public void PublishMovesDraftPostToPublishedRecordsTimestampAndRaisesEvent()
    {
        var postedByUserId = UserId.NewId();
        var post = TestDomain.CreatePost(postedByUserId);
        post.ClearDomainEvents();

        post.Publish(postedByUserId);

        Assert.AreEqual("Published", post.StateName);
        Assert.IsNotNull(post.PublishedOnUtc);
        Assert.IsNull(post.ArchivedOnUtc);
        Assert.IsTrue(post.PublishedOnUtc >= post.CreatedOnUtc);
        var domainEvent = (PostPublishedDomainEvent)post.DomainEvents.Single();
        Assert.AreEqual(post.PublishedOnUtc, domainEvent.OccurredOnUtc);
        Assert.AreEqual(post.Id, domainEvent.PostId);
        Assert.AreEqual(post.BlogId, domainEvent.BlogId);
        Assert.AreEqual(postedByUserId, domainEvent.PublishedByUserId);
    }

    [TestMethod]
    public void PublishFailsWhenPostIsArchived()
    {
        var userId = UserId.NewId();
        var post = TestDomain.CreatePost(userId);
        post.Publish(userId);
        post.Archive(userId);

        var exception = Assert.ThrowsExactly<DomainException>(() => post.Publish(userId));

        Assert.AreEqual("Archived posts cannot be published.", exception.Message);
    }

    [TestMethod]
    public void ArchiveMovesPublishedPostToArchivedRecordsTimestampAndRaisesEvent()
    {
        var userId = UserId.NewId();
        var post = TestDomain.CreatePost(userId);
        post.Publish(userId);
        post.ClearDomainEvents();

        post.Archive(userId);

        Assert.AreEqual("Archived", post.StateName);
        Assert.IsNotNull(post.PublishedOnUtc);
        Assert.IsNotNull(post.ArchivedOnUtc);
        Assert.IsTrue(post.ArchivedOnUtc >= post.PublishedOnUtc);
        var domainEvent = (PostArchivedDomainEvent)post.DomainEvents.Single();
        Assert.AreEqual(post.ArchivedOnUtc, domainEvent.OccurredOnUtc);
        Assert.AreEqual(post.Id, domainEvent.PostId);
        Assert.AreEqual(post.BlogId, domainEvent.BlogId);
        Assert.AreEqual(userId, domainEvent.ArchivedByUserId);
    }
}
