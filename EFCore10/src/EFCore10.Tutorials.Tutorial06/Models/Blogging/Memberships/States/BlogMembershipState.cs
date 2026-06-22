namespace EFCore10.Tutorials.Tutorial06.Models;

public abstract class BlogMembershipState
{
    public abstract string Key { get; }

    public abstract string Name { get; }

    public virtual bool IsActive => false;

    public virtual bool CanPost => false;

    public virtual BlogMembershipState Activate() =>
        throw new DomainException($"Blog membership cannot be activated when it is {Name}.");

    public virtual BlogMembershipState Revoke() => new RevokedBlogMembershipState();

    public virtual BlogMembershipState End() => new EndedBlogMembershipState();
}
