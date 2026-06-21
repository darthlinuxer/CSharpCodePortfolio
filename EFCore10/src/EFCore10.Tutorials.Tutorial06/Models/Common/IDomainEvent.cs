namespace EFCore10.Tutorials.Tutorial06.Models;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
