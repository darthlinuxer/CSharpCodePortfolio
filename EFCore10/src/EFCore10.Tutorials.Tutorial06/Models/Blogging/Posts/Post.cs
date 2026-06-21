namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class Post : AggregateRoot<PostId>
{
    private PostState _state = new DraftPostState();

    private Post()
    {
    }

    private Post(BlogId blogId, PostTitle title, PostContent content)
    {
        Id = PostId.NewId();
        BlogId = blogId;
        Title = title;
        Content = content;
    }

    public PostTitle Title { get; private set; } = null!;

    public PostContent Content { get; private set; } = null!;

    public BlogId BlogId { get; private set; }

    public Blog Blog { get; private set; } = null!;

    public string StateName => _state.Name;

    private string StateKey
    {
        get => _state.Key;
        set => _state = PostStateRegistry.FromKey(value);
    }

    public static Post Create(BlogId blogId, PostTitle title, PostContent content)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(content);

        var post = new Post(blogId, title, content);
        post.Raise(new PostCreatedDomainEvent(post.Id, blogId, DateTime.UtcNow));

        return post;
    }

    public void Publish()
    {
        _state = _state.Publish();
        Raise(new PostPublishedDomainEvent(Id, DateTime.UtcNow));
    }

    public void Archive()
    {
        _state = _state.Archive();
        Raise(new PostArchivedDomainEvent(Id, DateTime.UtcNow));
    }
}
