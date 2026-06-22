namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class BlogOwner
{
    private BlogOwner()
    {
    }

    private BlogOwner(BlogId blogId, UserId userId, DateTime startedOnUtc)
    {
        Id = BlogOwnerId.NewId();
        BlogId = blogId;
        UserId = userId;
        StartedOnUtc = startedOnUtc;
    }

    public BlogOwnerId Id { get; private set; }

    public BlogId BlogId { get; private set; }

    public Blog Blog { get; private set; } = null!;

    public UserId UserId { get; private set; }

    public User User { get; private set; } = null!;

    public DateTime StartedOnUtc { get; private set; }

    public DateTime? EndedOnUtc { get; private set; }

    public bool IsActive => EndedOnUtc is null;

    internal static BlogOwner Create(BlogId blogId, UserId userId, DateTime startedOnUtc) =>
        new(blogId, userId, startedOnUtc);

    internal void End(DateTime endedOnUtc)
    {
        if (EndedOnUtc is not null)
            throw new DomainException("Blog owner is already inactive.");

        if (endedOnUtc < StartedOnUtc)
            throw new DomainException("Blog owner end date cannot be before start date.");

        EndedOnUtc = endedOnUtc;
    }
}
