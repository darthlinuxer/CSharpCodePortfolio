namespace EFCore10.Tutorials.Tutorial06.Models;

public abstract class PostState
{
    public abstract string Key { get; }

    public abstract string Name { get; }

    public virtual PostState Publish() =>
        throw new DomainException($"{Name} posts cannot be published.");

    public virtual PostState Archive() =>
        throw new DomainException($"{Name} posts cannot be archived.");

}
