using EFCore10.Tutorials.Tutorial06;
using EFCore10.Tutorials.Tutorial06.Models;
using EFCore10.Tutorials.Tutorial06.Persistence.Outbox;

namespace EFCore10.Tutorials.Tutorial06.Tests;

[TestClass]
public sealed class Tutorial06EvidenceFormatterTests
{
    [TestMethod]
    public void FormatsWorkflowStatesSeparatelyFromOutboxDispatchStatus()
    {
        var owner = TestDomain.CreateOwner();
        var authorUser = TestDomain.CreateAuthorUser();
        var blog = TestDomain.CreateBlog(owner);
        var author = blog.InviteAuthor(authorUser, owner);
        blog.AcceptAuthorInvitation(author.Id, authorUser);
        var post = blog.CreatePost(
            authorUser,
            PostTitle.Create("Workflow evidence"),
            PostContent.Create("Formatter must not mix domain workflow state with outbox dispatch status."));
        post.Publish(authorUser.Id);
        post.Archive(authorUser.Id);

        var outboxMessages = new[]
        {
            OutboxMessage.FromDomainEvent(blog.DomainEvents.OfType<BlogCreatedDomainEvent>().Single()),
            OutboxMessage.FromDomainEvent(post.DomainEvents.OfType<PostPublishedDomainEvent>().Single())
        };

        Assert.AreEqual(
            "Blog=Active; CurrentOwner=Owner/Active; Author=Author/Active; Post=Archived",
            Tutorial06EvidenceFormatter.FormatWorkflowStates(blog, author, post));
        Assert.AreEqual(
            "blog.created, post.published",
            Tutorial06EvidenceFormatter.FormatOutboxEventNames(outboxMessages));
        Assert.AreEqual(
            "Pending=2 (aguardando worker)",
            Tutorial06EvidenceFormatter.FormatOutboxDispatchStatus(outboxMessages));
    }
}
