namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogRenamedDomainEvent(
    BlogId BlogId,
    string Name,
    UserId RenamedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
