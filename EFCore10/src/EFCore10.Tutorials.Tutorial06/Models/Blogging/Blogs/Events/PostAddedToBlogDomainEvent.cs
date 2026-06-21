namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostAddedToBlogDomainEvent(
    BlogId BlogId,
    PostId PostId,
    DateTime OccurredOnUtc) : IDomainEvent;
