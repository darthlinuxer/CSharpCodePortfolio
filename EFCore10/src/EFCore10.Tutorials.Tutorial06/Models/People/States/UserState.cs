namespace EFCore10.Tutorials.Tutorial06.Models;

public abstract class UserState
{
    public abstract string Key { get; }

    public virtual bool CanLogin => false;

    public virtual UserState Activate() =>
        throw new DomainException("Only inactive users can be activated.");

    public virtual UserState Deactivate() =>
        throw new DomainException("Only active users can be deactivated.");

    public virtual UserState Lock() => new LockedUserState();

}
