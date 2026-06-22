namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogRenamedDomainEvent(
    BlogId BlogId,
    string Name,
    UserId RenamedByUserId,
    Timestamp OccurredOnUtc) : IDomainEvent
{
    public string EventName => "blog.renamed";

    public int EventVersion => 1;

    public string AggregateType => "Blog";

    public string AggregateId => BlogId.ToString();
}
