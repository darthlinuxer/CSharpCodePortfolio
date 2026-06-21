namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class Blog : AggregateRoot<BlogId>
{
    private readonly List<Post> _posts = [];

    private Blog()
    {
    }

    private Blog(BlogName name, BlogUrl url, Author author)
    {
        Id = BlogId.NewId();
        Name = name;
        Url = url;
        Author = author;
        AuthorId = author.Id;
    }

    public BlogName Name { get; private set; } = null!;

    public BlogUrl Url { get; private set; } = null!;

    public PersonId AuthorId { get; private set; }

    public Author Author { get; private set; } = null!;

    public IReadOnlyCollection<Post> Posts => _posts;

    public static Blog Create(BlogName name, BlogUrl url, Author author)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(author);

        var blog = new Blog(name, url, author);
        blog.Raise(new BlogCreatedDomainEvent(blog.Id, author.Id, DateTime.UtcNow));

        return blog;
    }

    public Post AddPost(PostTitle title, PostContent content)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(content);

        var post = Post.Create(Id, title, content);
        _posts.Add(post);
        Raise(new PostAddedToBlogDomainEvent(Id, post.Id, DateTime.UtcNow));

        return post;
    }
}
