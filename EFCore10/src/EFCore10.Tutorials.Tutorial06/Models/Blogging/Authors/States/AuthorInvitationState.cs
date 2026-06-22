namespace EFCore10.Tutorials.Tutorial06.Models;

public abstract class AuthorInvitationState
{
    public abstract string Key { get; }

    public abstract string Name { get; }

    public virtual bool CanPost => false;

    public virtual AuthorInvitationState Accept() =>
        throw new DomainException($"Author invitation cannot be accepted when it is {Name}.");

    public virtual AuthorInvitationState Revoke() => new RevokedAuthorInvitationState();
}
