namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class Author
{
    private AuthorInvitationState _state = new PendingAuthorInvitationState();

    private Author()
    {
    }

    private Author(BlogId blogId, UserId userId)
    {
        Id = AuthorId.NewId();
        BlogId = blogId;
        UserId = userId;
    }

    public AuthorId Id { get; private set; }

    public BlogId BlogId { get; private set; }

    public Blog Blog { get; private set; } = null!;

    public UserId UserId { get; private set; }

    public User User { get; private set; } = null!;

    public string StateName => _state.Name;

    public bool CanPost => _state.CanPost;

    private string StateKey
    {
        get => _state.Key;
        set => _state = AuthorInvitationStateRegistry.FromKey(value);
    }

    internal static Author Invite(BlogId blogId, UserId userId) => new(blogId, userId);

    internal void Accept() => _state = _state.Accept();

    internal void Revoke() => _state = _state.Revoke();
}
