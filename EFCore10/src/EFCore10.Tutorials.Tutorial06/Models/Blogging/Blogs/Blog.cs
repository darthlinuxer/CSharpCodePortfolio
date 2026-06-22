namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class Blog : AggregateRoot<BlogId>
{
    private readonly List<Post> _posts = [];
    private readonly List<BlogOwner> _owners = [];
    private readonly List<Author> _authors = [];
    private BlogState _state = new ActiveBlogState();

    private Blog()
    {
    }

    private Blog(BlogName name, BlogUrl url, User owner, DateTime occurredOnUtc)
    {
        Id = BlogId.NewId();
        Name = name;
        Url = url;

        var blogOwner = BlogOwner.Create(Id, owner.Id, occurredOnUtc);
        _owners.Add(blogOwner);
    }

    public BlogName Name { get; private set; } = null!;

    public BlogUrl Url { get; private set; } = null!;

    public IReadOnlyCollection<BlogOwner> Owners => _owners;

    public IReadOnlyCollection<Author> Authors => _authors;

    public IReadOnlyCollection<Post> Posts => _posts;

    public BlogOwner CurrentOwner => _owners.Single(owner => owner.IsActive);

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

        var occurredOnUtc = DateTime.UtcNow;
        var blog = new Blog(name, url, owner, occurredOnUtc);
        blog.Raise(new BlogCreatedDomainEvent(blog.Id, blog.CurrentOwner.Id, owner.Id, occurredOnUtc));

        return blog;
    }

    public void Rename(BlogName name)
    {
        _state.EnsureAllowsChanges();
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
        Raise(new BlogRenamedDomainEvent(Id, name.Value, DateTime.UtcNow));
    }

    public void TransferOwnership(User newOwner)
    {
        _state.EnsureAllowsChanges();
        ArgumentNullException.ThrowIfNull(newOwner);

        var currentOwner = CurrentOwner;
        if (currentOwner.UserId == newOwner.Id)
            throw new DomainException("New blog owner must be different from the current owner.");

        var occurredOnUtc = DateTime.UtcNow;
        currentOwner.End(occurredOnUtc);

        var newBlogOwner = BlogOwner.Create(Id, newOwner.Id, occurredOnUtc);
        _owners.Add(newBlogOwner);

        Raise(new BlogOwnershipTransferredDomainEvent(
            Id,
            currentOwner.Id,
            newBlogOwner.Id,
            currentOwner.UserId,
            newOwner.Id,
            occurredOnUtc));
    }

    public void Delete(User deletedBy)
    {
        _state.EnsureAllowsChanges();
        EnsureOwner(deletedBy);

        _state = _state.Delete();
    }

    public Author InviteAuthor(User user)
    {
        _state.EnsureAllowsChanges();
        ArgumentNullException.ThrowIfNull(user);

        if (CurrentOwner.UserId == user.Id)
            throw new DomainException("Blog owner already has permission to post.");

        if (_authors.Any(author => author.UserId == user.Id))
            throw new DomainException("User already has an author invitation for this blog.");

        var author = Author.Invite(Id, user.Id);
        _authors.Add(author);
        Raise(new AuthorInvitedToBlogDomainEvent(Id, author.Id, user.Id, CurrentOwner.UserId, DateTime.UtcNow));

        return author;
    }

    public void AcceptAuthor(AuthorId authorId)
    {
        _state.EnsureAllowsChanges();

        var author = GetAuthor(authorId);
        author.Accept();
        Raise(new AuthorAcceptedBlogInvitationDomainEvent(Id, author.Id, author.UserId, DateTime.UtcNow));
    }

    public void RevokeAuthor(AuthorId authorId, User revokedBy)
    {
        _state.EnsureAllowsChanges();
        EnsureOwner(revokedBy);

        var author = GetAuthor(authorId);
        author.Revoke();
        Raise(new AuthorRevokedFromBlogDomainEvent(Id, author.Id, author.UserId, revokedBy.Id, DateTime.UtcNow));
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

        if (_authors.Any(author => author.UserId == userId && author.CanPost))
            return;

        throw new DomainException("User cannot post to this blog.");
    }

    private Author GetAuthor(AuthorId authorId) =>
        _authors.SingleOrDefault(author => author.Id == authorId)
        ?? throw new DomainException("Author invitation was not found for this blog.");
}
