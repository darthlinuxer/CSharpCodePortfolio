using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Tests;

[TestClass]
public sealed class DomainEventTests
{
    [TestMethod]
    public void UserRegisterRaisesUserRegisteredDomainEvent()
    {
        var user = TestDomain.CreateOwner();

        var domainEvent = AssertHasSingleEvent<UserRegisteredDomainEvent>(user);
        Assert.AreEqual(user.Id, domainEvent.UserId);
    }

    [TestMethod]
    public void BlogCreateRaisesBlogCreatedDomainEvent()
    {
        var owner = TestDomain.CreateOwner();
        var blog = TestDomain.CreateBlog(owner);

        var domainEvent = AssertHasSingleEvent<BlogCreatedDomainEvent>(blog);
        Assert.AreEqual(blog.Id, domainEvent.BlogId);
        Assert.AreEqual(blog.CurrentOwner.Id, domainEvent.BlogOwnerId);
        Assert.AreEqual(owner.Id, domainEvent.OwnerUserId);
    }

    [TestMethod]
    public void BlogInvitesAndAcceptsAuthor()
    {
        var blog = TestDomain.CreateBlog(TestDomain.CreateOwner());
        var user = TestDomain.CreateAuthorUser();

        var author = blog.InviteAuthor(user);
        blog.AcceptAuthor(author.Id);

        var invitedEvent = blog.DomainEvents.OfType<AuthorInvitedToBlogDomainEvent>().Single();
        var acceptedEvent = blog.DomainEvents.OfType<AuthorAcceptedBlogInvitationDomainEvent>().Single();

        Assert.AreEqual(author.Id, invitedEvent.AuthorId);
        Assert.AreEqual(user.Id, invitedEvent.InvitedUserId);
        Assert.AreEqual(author.Id, acceptedEvent.AuthorId);
        Assert.AreEqual(user.Id, acceptedEvent.AuthorUserId);
        Assert.AreEqual("Accepted", author.StateName);
    }

    [TestMethod]
    public void BlogTransferOwnershipEndsPreviousOwnerAndCreatesNewActiveOwner()
    {
        var owner = TestDomain.CreateOwner();
        var successor = TestDomain.CreateSuccessorOwner();
        var blog = TestDomain.CreateBlog(owner);

        var previousOwner = blog.CurrentOwner;
        blog.TransferOwnership(successor);

        var transferredEvent = blog.DomainEvents.OfType<BlogOwnershipTransferredDomainEvent>().Single();

        Assert.IsFalse(previousOwner.IsActive);
        Assert.AreEqual(successor.Id, blog.CurrentOwner.UserId);
        Assert.AreEqual(previousOwner.Id, transferredEvent.PreviousBlogOwnerId);
        Assert.AreEqual(blog.CurrentOwner.Id, transferredEvent.NewBlogOwnerId);
    }

    [TestMethod]
    public void OwnerCanCreatePostWithoutAuthorRole()
    {
        var owner = TestDomain.CreateOwner();
        var blog = TestDomain.CreateBlog(owner);

        var post = blog.CreatePost(
            owner,
            PostTitle.Create("Owner post"),
            PostContent.Create("Owner can post without author invite."));

        var createdEvent = AssertHasSingleEvent<PostCreatedDomainEvent>(post);

        Assert.AreEqual(owner.Id, post.PostedByUserId);
        Assert.AreEqual(owner.Id, createdEvent.PostedByUserId);
    }

    [TestMethod]
    public void UserWithoutBlogRoleCannotCreatePost()
    {
        var blog = TestDomain.CreateBlog(TestDomain.CreateOwner());
        var user = TestDomain.CreateAuthorUser();

        var exception = Assert.ThrowsExactly<DomainException>(() => blog.CreatePost(
            user,
            PostTitle.Create("Unauthorized"),
            PostContent.Create("This user has no role.")));

        Assert.AreEqual("User cannot post to this blog.", exception.Message);
    }

    [TestMethod]
    public void RevokedAuthorCannotCreatePost()
    {
        var owner = TestDomain.CreateOwner();
        var user = TestDomain.CreateAuthorUser();
        var blog = TestDomain.CreateBlog(owner);
        var author = blog.InviteAuthor(user);
        blog.AcceptAuthor(author.Id);
        blog.RevokeAuthor(author.Id, owner);

        var exception = Assert.ThrowsExactly<DomainException>(() => blog.CreatePost(
            user,
            PostTitle.Create("Revoked"),
            PostContent.Create("Revoked authors cannot post.")));

        Assert.AreEqual("User cannot post to this blog.", exception.Message);
    }

    [TestMethod]
    public void ClearDomainEventsEmptiesInMemoryEvents()
    {
        var author = TestDomain.CreateOwner();

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
