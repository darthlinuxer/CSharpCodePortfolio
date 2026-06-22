namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class Blog : AggregateRoot<BlogId>
{
    private readonly List<Post> _posts = [];
    private readonly List<BlogMembership> _memberships = [];
    private BlogState _state = new ActiveBlogState();

    private Blog()
    {
    }

    private Blog(BlogName name, BlogUrl url, User owner, Timestamp occurredOnUtc)
    {
        Id = BlogId.NewId();
        Name = name;
        Url = url;

        var ownerMembership = BlogMembership.CreateOwner(Id, owner.Id, occurredOnUtc, owner.Id);
        _memberships.Add(ownerMembership);
    }

    public BlogName Name { get; private set; } = null!;

    public BlogUrl Url { get; private set; } = null!;

    public IReadOnlyCollection<BlogMembership> Memberships => _memberships;

    public IReadOnlyCollection<BlogMembership> Authors => _memberships
        .Where(membership => membership.IsAuthor())
        .ToArray();

    public IReadOnlyCollection<Post> Posts => _posts;

    public BlogMembership CurrentOwner => _memberships.Single(membership => membership.IsOwner() && membership.IsActive);

    public string StateName => _state.Name;

    private string StateKey
    {
        get => _state.Key;
        set => _state = BlogStateRegistry.FromKey(value);
    }

    public static Blog Create(BlogName name, BlogUrl url, User owner)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(owner);

        var occurredOnUtc = Timestamp.UtcNow;
        var blog = new Blog(name, url, owner, occurredOnUtc);
        blog.Raise(new BlogCreatedDomainEvent(blog.Id, blog.CurrentOwner.Id, owner.Id, occurredOnUtc));

        return blog;
    }

    public void Rename(BlogName name, User renamedBy)
    {
        _state.EnsureAllowsChanges();
        ArgumentNullException.ThrowIfNull(name);
        EnsureOwner(renamedBy);

        Name = name;
        Raise(new BlogRenamedDomainEvent(Id, name.Value, renamedBy.Id, Timestamp.UtcNow));
    }

    public void TransferOwnership(User newOwner, User transferredBy)
    {
        _state.EnsureAllowsChanges();
        ArgumentNullException.ThrowIfNull(newOwner);
        EnsureOwner(transferredBy);

        var currentOwner = CurrentOwner;
        if (currentOwner.UserId == newOwner.Id)
            throw new DomainException("New blog owner must be different from the current owner.");

        var occurredOnUtc = Timestamp.UtcNow;
        currentOwner.End(transferredBy.Id, occurredOnUtc);

        var newOwnerMembership = BlogMembership.CreateOwner(Id, newOwner.Id, occurredOnUtc, transferredBy.Id);
        _memberships.Add(newOwnerMembership);

        Raise(new BlogOwnershipTransferredDomainEvent(
            Id,
            currentOwner.Id,
            newOwnerMembership.Id,
            currentOwner.UserId,
            newOwner.Id,
            transferredBy.Id,
            occurredOnUtc));
    }

    public void Delete(User deletedBy)
    {
        _state.EnsureAllowsChanges();
        EnsureOwner(deletedBy);

        _state = _state.Delete();
        Raise(new BlogDeletedDomainEvent(Id, deletedBy.Id, Timestamp.UtcNow));
    }

    public BlogMembership InviteAuthor(User user, User invitedBy)
    {
        _state.EnsureAllowsChanges();
        ArgumentNullException.ThrowIfNull(user);
        EnsureOwner(invitedBy);

        if (CurrentOwner.UserId == user.Id)
            throw new DomainException("Blog owner already has permission to post.");

        if (_memberships.Any(membership => membership.UserId == user.Id && membership.IsAuthor() && membership.StateName is "Pending" or "Active"))
            throw new DomainException("User already has an author invitation for this blog.");

        var occurredOnUtc = Timestamp.UtcNow;
        var membership = BlogMembership.InviteAuthor(Id, user.Id, invitedBy.Id, occurredOnUtc);
        _memberships.Add(membership);
        Raise(new AuthorInvitedToBlogDomainEvent(Id, membership.Id, user.Id, invitedBy.Id, occurredOnUtc));

        return membership;
    }

    public void AcceptAuthorInvitation(BlogMembershipId membershipId, User acceptedBy)
    {
        _state.EnsureAllowsChanges();
        ArgumentNullException.ThrowIfNull(acceptedBy);

        var occurredOnUtc = Timestamp.UtcNow;
        var membership = GetAuthorMembership(membershipId);
        membership.Activate(acceptedBy.Id, occurredOnUtc);
        Raise(new AuthorAcceptedBlogInvitationDomainEvent(Id, membership.Id, membership.UserId, acceptedBy.Id, occurredOnUtc));
    }

    public void RevokeAuthor(BlogMembershipId membershipId, User revokedBy)
    {
        _state.EnsureAllowsChanges();
        EnsureOwner(revokedBy);

        var occurredOnUtc = Timestamp.UtcNow;
        var membership = GetAuthorMembership(membershipId);
        membership.Revoke(revokedBy.Id, occurredOnUtc);
        Raise(new AuthorRevokedFromBlogDomainEvent(Id, membership.Id, membership.UserId, revokedBy.Id, occurredOnUtc));
    }

    public Post CreatePost(User postedBy, PostTitle title, PostContent content)
    {
        _state.EnsureAllowsChanges();
        ArgumentNullException.ThrowIfNull(postedBy);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(content);

        EnsureCanPost(postedBy.Id);

        var post = Post.Create(Id, postedBy.Id, title, content);
        _posts.Add(post);

        return post;
    }

    private void EnsureOwner(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (CurrentOwner.UserId != user.Id)
            throw new DomainException("Only the current blog owner can perform this operation.");
    }

    private void EnsureCanPost(UserId userId)
    {
        if (CurrentOwner.UserId == userId)
            return;

        if (_memberships.Any(membership => membership.UserId == userId && membership.CanPost))
            return;

        throw new DomainException("User cannot post to this blog.");
    }

    private BlogMembership GetAuthorMembership(BlogMembershipId membershipId) =>
        _memberships.SingleOrDefault(membership => membership.Id == membershipId && membership.IsAuthor())
        ?? throw new DomainException("Author invitation was not found for this blog.");
}
