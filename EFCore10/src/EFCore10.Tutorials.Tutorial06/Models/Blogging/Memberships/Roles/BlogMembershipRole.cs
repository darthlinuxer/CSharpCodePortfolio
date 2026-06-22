namespace EFCore10.Tutorials.Tutorial06.Models;

public abstract class BlogMembershipRole
{
    public abstract string Key { get; }

    public abstract string Name { get; }

    public virtual bool CanPostWhenActive => true;
}
