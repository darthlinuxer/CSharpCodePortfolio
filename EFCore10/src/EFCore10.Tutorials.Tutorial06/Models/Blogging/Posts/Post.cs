namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class Post : AggregateRoot<PostId>
{
    private PostState _state = new DraftPostState();

    private Post()
    {
    }

    private Post(BlogId blogId, UserId postedByUserId, PostTitle title, PostContent content, Timestamp createdOnUtc)
    {
        Id = PostId.NewId();
        BlogId = blogId;
        PostedByUserId = postedByUserId;
        Title = title;
        Content = content;
        CreatedOnUtc = createdOnUtc;
    }

    public PostTitle Title { get; private set; } = null!;

    public PostContent Content { get; private set; } = null!;

    public BlogId BlogId { get; private set; }

    public Blog Blog { get; private set; } = null!;

    public UserId PostedByUserId { get; private set; }

    public User PostedBy { get; private set; } = null!;

    public Timestamp CreatedOnUtc { get; private set; }

    public Timestamp? PublishedOnUtc { get; private set; }

    public Timestamp? ArchivedOnUtc { get; private set; }

    public string StateName => _state.Name;

    private string StateKey
    {
        get => _state.Key;
        set => _state = PostStateRegistry.FromKey(value);
    }

    public static Post Create(BlogId blogId, UserId postedByUserId, PostTitle title, PostContent content)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(content);

        var occurredOnUtc = Timestamp.UtcNow;
        var post = new Post(blogId, postedByUserId, title, content, occurredOnUtc);
        post.Raise(new PostDraftCreatedDomainEvent(post.Id, blogId, postedByUserId, occurredOnUtc));

        return post;
    }

    public void Publish(UserId publishedByUserId)
    {
        var occurredOnUtc = Timestamp.UtcNow;
        _state = _state.Publish();
        PublishedOnUtc = occurredOnUtc;
        Raise(new PostPublishedDomainEvent(Id, BlogId, publishedByUserId, occurredOnUtc));
    }

    public void Archive(UserId archivedByUserId)
    {
        var occurredOnUtc = Timestamp.UtcNow;
        _state = _state.Archive();
        ArchivedOnUtc = occurredOnUtc;
        Raise(new PostArchivedDomainEvent(Id, BlogId, archivedByUserId, occurredOnUtc));
    }
}
