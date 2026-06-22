namespace EFCore10.Tutorials.Tutorial06.Models;

public abstract class BlogState
{
    public abstract string Key { get; }

    public abstract string Name { get; }

    public virtual bool AllowsChanges => true;

    public virtual BlogState Delete() => new DeletedBlogState();

    public void EnsureAllowsChanges()
    {
        if (!AllowsChanges)
            throw new DomainException($"Blog cannot be changed when it is {Name}.");
    }
}
