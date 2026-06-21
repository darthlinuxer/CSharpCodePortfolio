namespace EFCore10.Tutorials.Tutorial06.Models;

public abstract class AggregateRoot<TId> : AggregateRoot
    where TId : struct
{
    public TId Id { get; protected set; }
}
