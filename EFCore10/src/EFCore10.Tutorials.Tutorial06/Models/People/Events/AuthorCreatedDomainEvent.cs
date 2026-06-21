namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record AuthorCreatedDomainEvent(
    AuthorId AuthorId,
    DateTime OccurredOnUtc) : IDomainEvent;
