namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record UserRegisteredDomainEvent(
    UserId UserId,
    Timestamp OccurredOnUtc) : IDomainEvent;
