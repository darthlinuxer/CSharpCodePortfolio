using EFCore10.Tutorials.Tutorial06.Models;
using EFCore10.Tutorials.Tutorial06.Persistence.Outbox;

namespace EFCore10.Tutorials.Tutorial06;

public static class Tutorial06EvidenceFormatter
{
    public static string FormatWorkflowStates(Blog blog, BlogMembership author, Post post) =>
        $"Blog={blog.StateName}; CurrentOwner={blog.CurrentOwner.RoleName}/{blog.CurrentOwner.StateName}; " +
        $"Author={author.RoleName}/{author.StateName}; Post={post.StateName}";

    public static string FormatDomainEvents(IEnumerable<AggregateRoot> aggregates)
    {
        var eventNames = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .Select(domainEvent => domainEvent.GetType().Name)
            .ToArray();

        return eventNames.Length == 0 ? "(nenhum)" : string.Join(", ", eventNames);
    }

    public static string FormatOutboxEventNames(IEnumerable<OutboxMessage> messages)
    {
        var eventNames = messages
            .Select(message => message.EventName)
            .ToArray();

        return eventNames.Length == 0 ? "(nenhum)" : string.Join(", ", eventNames);
    }

    public static string FormatOutboxDispatchStatus(IEnumerable<OutboxMessage> messages)
    {
        var statuses = messages
            .GroupBy(message => message.Status)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => FormatOutboxStatusGroup(group.Key, group.Count()))
            .ToArray();

        return statuses.Length == 0 ? "(nenhum)" : string.Join(", ", statuses);
    }

    private static string FormatOutboxStatusGroup(string status, int count) =>
        status == "Pending"
            ? $"{status}={count} (aguardando worker)"
            : $"{status}={count}";
}
