namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class BlogMembership
{
    private BlogMembershipRole _role = new AuthorBlogMembershipRole();
    private BlogMembershipState _state = new PendingBlogMembershipState();

    private BlogMembership()
    {
    }

    private BlogMembership(
        BlogId blogId,
        UserId userId,
        BlogMembershipRole role,
        BlogMembershipState state,
        Timestamp createdOnUtc,
        Timestamp? activatedOnUtc,
        UserId? createdByUserId)
    {
        Id = BlogMembershipId.NewId();
        BlogId = blogId;
        UserId = userId;
        _role = role;
        _state = state;
        CreatedOnUtc = createdOnUtc;
        ActivatedOnUtc = activatedOnUtc;
        CreatedByUserId = createdByUserId;
    }

    public BlogMembershipId Id { get; private set; }

    public BlogId BlogId { get; private set; }

    public Blog Blog { get; private set; } = null!;

    public UserId UserId { get; private set; }

    public User User { get; private set; } = null!;

    public Timestamp CreatedOnUtc { get; private set; }

    public Timestamp? ActivatedOnUtc { get; private set; }

    public Timestamp? EndedOnUtc { get; private set; }

    public UserId? CreatedByUserId { get; private set; }

    public UserId? EndedByUserId { get; private set; }

    public string RoleName => _role.Name;

    public string StateName => _state.Name;

    public bool IsActive => _state.IsActive;

    public bool CanPost => _role.CanPostWhenActive && _state.CanPost;

    private string RoleKey
    {
        get => _role.Key;
        set => _role = BlogMembershipRoleRegistry.FromKey(value);
    }

    private string StateKey
    {
        get => _state.Key;
        set => _state = BlogMembershipStateRegistry.FromKey(value);
    }

    internal static BlogMembership CreateOwner(BlogId blogId, UserId userId, Timestamp occurredOnUtc, UserId? createdByUserId) =>
        new(blogId, userId, new OwnerBlogMembershipRole(), new ActiveBlogMembershipState(), occurredOnUtc, occurredOnUtc, createdByUserId);

    internal static BlogMembership InviteAuthor(BlogId blogId, UserId userId, UserId invitedByUserId, Timestamp occurredOnUtc) =>
        new(blogId, userId, new AuthorBlogMembershipRole(), new PendingBlogMembershipState(), occurredOnUtc, null, invitedByUserId);

    internal void Activate(UserId activatedByUserId, Timestamp occurredOnUtc)
    {
        if (UserId != activatedByUserId)
            throw new DomainException("Only the invited user can accept the author invitation.");

        _state = _state.Activate();
        ActivatedOnUtc ??= occurredOnUtc;
    }

    internal void Revoke(UserId revokedByUserId, Timestamp occurredOnUtc)
    {
        EnsureAuthor();

        _state = _state.Revoke();
        EndedOnUtc ??= occurredOnUtc;
        EndedByUserId ??= revokedByUserId;
    }

    internal void End(UserId endedByUserId, Timestamp occurredOnUtc)
    {
        _state = _state.End();
        EndedOnUtc ??= occurredOnUtc;
        EndedByUserId ??= endedByUserId;
    }

    internal bool IsOwner() => _role is OwnerBlogMembershipRole;

    internal bool IsAuthor() => _role is AuthorBlogMembershipRole;

    private void EnsureAuthor()
    {
        if (!IsAuthor())
            throw new DomainException("Only author memberships can be revoked as authors.");
    }
}
