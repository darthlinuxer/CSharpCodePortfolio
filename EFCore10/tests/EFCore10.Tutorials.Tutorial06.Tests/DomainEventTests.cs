using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Tests;

[TestClass]
public sealed class DomainEventTests
{
    [TestMethod]
    public void DomainEventContractContainsOnlyOccurrenceTimestamp()
    {
        var propertyNames = typeof(IDomainEvent)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        CollectionAssert.AreEqual(new[] { nameof(IDomainEvent.OccurredOnUtc) }, propertyNames);
    }

    [TestMethod]
    public void UserRegisterRaisesUserRegisteredDomainEvent()
    {
        var user = TestDomain.CreateOwner();

        var domainEvent = AssertHasSingleEvent<UserRegisteredDomainEvent>(user);
        Assert.AreEqual(user.Id, domainEvent.UserId);
        Assert.AreNotEqual(default, domainEvent.OccurredOnUtc);
    }

    [TestMethod]
    public void BlogCreateRaisesBlogCreatedDomainEventAndCreatesSingleActiveOwnerMembership()
    {
        var owner = TestDomain.CreateOwner();
        var blog = TestDomain.CreateBlog(owner);

        var domainEvent = AssertHasSingleEvent<BlogCreatedDomainEvent>(blog);
        var currentOwner = blog.CurrentOwner;

        Assert.AreEqual(blog.Id, domainEvent.BlogId);
        Assert.AreEqual(currentOwner.Id, domainEvent.OwnerMembershipId);
        Assert.AreEqual(owner.Id, domainEvent.OwnerUserId);
        Assert.AreNotEqual(default, domainEvent.OccurredOnUtc);
        Assert.AreEqual("Owner", currentOwner.RoleName);
        Assert.AreEqual("Active", currentOwner.StateName);
        Assert.HasCount(1, blog.Memberships.Where(membership => membership.RoleName == "Owner" && membership.IsActive).ToArray());
    }

    [TestMethod]
    public void BlogInvitesAndAcceptsAuthorMembership()
    {
        var owner = TestDomain.CreateOwner();
        var blog = TestDomain.CreateBlog(owner);
        var user = TestDomain.CreateAuthorUser();

        var membership = blog.InviteAuthor(user, owner);
        blog.AcceptAuthorInvitation(membership.Id, user);

        var invitedEvent = blog.DomainEvents.OfType<AuthorInvitedToBlogDomainEvent>().Single();
        var acceptedEvent = blog.DomainEvents.OfType<AuthorAcceptedBlogInvitationDomainEvent>().Single();

        Assert.AreEqual(membership.Id, invitedEvent.AuthorMembershipId);
        Assert.AreEqual(user.Id, invitedEvent.InvitedUserId);
        Assert.AreEqual(owner.Id, invitedEvent.InvitedByUserId);
        Assert.AreEqual(membership.Id, acceptedEvent.AuthorMembershipId);
        Assert.AreEqual(user.Id, acceptedEvent.AcceptedByUserId);
        Assert.AreEqual("Author", membership.RoleName);
        Assert.AreEqual("Active", membership.StateName);
        Assert.IsTrue(membership.CanPost);
    }

    [TestMethod]
    public void BlogTransferOwnershipEndsPreviousOwnerAndCreatesNewActiveOwner()
    {
        var owner = TestDomain.CreateOwner();
        var successor = TestDomain.CreateSuccessorOwner();
        var blog = TestDomain.CreateBlog(owner);

        var previousOwner = blog.CurrentOwner;
        blog.TransferOwnership(successor, owner);

        var transferredEvent = blog.DomainEvents.OfType<BlogOwnershipTransferredDomainEvent>().Single();

        Assert.IsFalse(previousOwner.IsActive);
        Assert.AreEqual("Ended", previousOwner.StateName);
        Assert.IsNotNull(previousOwner.EndedOnUtc);
        Assert.AreEqual(successor.Id, blog.CurrentOwner.UserId);
        Assert.AreEqual(previousOwner.Id, transferredEvent.PreviousOwnerMembershipId);
        Assert.AreEqual(blog.CurrentOwner.Id, transferredEvent.NewOwnerMembershipId);
        Assert.AreEqual(owner.Id, transferredEvent.TransferredByUserId);
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

        var createdEvent = AssertHasSingleEvent<PostDraftCreatedDomainEvent>(post);

        Assert.IsFalse(blog.Authors.Any(membership => membership.UserId == owner.Id));
        Assert.AreEqual(owner.Id, post.PostedByUserId);
        Assert.AreEqual(owner.Id, createdEvent.PostedByUserId);
        Assert.AreEqual(blog.Id, createdEvent.BlogId);
        Assert.IsTrue(post.CreatedOnUtc <= Timestamp.UtcNow);
        Assert.IsNull(post.PublishedOnUtc);
        Assert.IsNull(post.ArchivedOnUtc);
    }

    [TestMethod]
    public void UserWithoutBlogMembershipCannotCreatePost()
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
        var membership = blog.InviteAuthor(user, owner);
        blog.AcceptAuthorInvitation(membership.Id, user);
        blog.RevokeAuthor(membership.Id, owner);

        var exception = Assert.ThrowsExactly<DomainException>(() => blog.CreatePost(
            user,
            PostTitle.Create("Revoked"),
            PostContent.Create("Revoked authors cannot post.")));

        Assert.AreEqual("Revoked", membership.StateName);
        Assert.IsFalse(membership.CanPost);
        Assert.AreEqual("User cannot post to this blog.", exception.Message);
    }

    [TestMethod]
    public void BlogDeleteRaisesBlogDeletedDomainEvent()
    {
        var owner = TestDomain.CreateOwner();
        var blog = TestDomain.CreateBlog(owner);

        blog.Delete(owner);

        var deletedEvent = blog.DomainEvents.OfType<BlogDeletedDomainEvent>().Single();
        Assert.AreEqual(blog.Id, deletedEvent.BlogId);
        Assert.AreEqual(owner.Id, deletedEvent.DeletedByUserId);
        Assert.AreEqual("Deleted", blog.StateName);
    }

    [TestMethod]
    public void ClearDomainEventsEmptiesInMemoryEvents()
    {
        var user = TestDomain.CreateOwner();

        user.ClearDomainEvents();

        Assert.IsFalse(user.DomainEvents.Any());
    }

    private static TEvent AssertHasSingleEvent<TEvent>(AggregateRoot aggregate)
        where TEvent : IDomainEvent
    {
        var domainEvent = aggregate.DomainEvents.OfType<TEvent>().Single();
        Assert.IsInstanceOfType(domainEvent, typeof(TEvent));

        return domainEvent;
    }
}
