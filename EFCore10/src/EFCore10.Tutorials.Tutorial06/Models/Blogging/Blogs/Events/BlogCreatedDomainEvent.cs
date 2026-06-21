namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogCreatedDomainEvent(
    BlogId BlogId,
    PersonId AuthorId,
    DateTime OccurredOnUtc) : IDomainEvent;
