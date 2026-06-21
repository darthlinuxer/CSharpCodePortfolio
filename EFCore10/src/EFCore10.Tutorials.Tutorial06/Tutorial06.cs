using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial06.Models;
using EFCore10.Tutorials.Tutorial06.Persistence;
using EFCore10.Tutorials.Tutorial06.Persistence.Outbox;
using EFCore10.Tutorials.Tutorial06.SampleData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Tutorials.Tutorial06;

[Tutorial("06", "ddd-value-objects", "DDD value objects, states, and SQLite persistence")]
public sealed class Tutorial06 : ITutorial
{
    private const string ConnectionStringName = "TutorialDatabase";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(Tutorial06).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        TutorialConsole.WriteHeader("06", "DDD + EF Core 10 + SQLite");
        TutorialConsole.WriteContext(("SQLite", connectionString));

        var services = new ServiceCollection()
            .AddSqliteDbContext<BloggingContext>(connectionString);

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BloggingContext>();

        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var author = Tutorial06SampleData.CreateDemoAuthor();
        var blog = Tutorial06SampleData.CreateDemoBlog(author);
        var post = Tutorial06SampleData.AddArchivedPost(blog);

        var aggregates = new AggregateRoot[] { author, blog, post };
        var capturedEvents = FormatDomainEvents(aggregates);

        context.AddRange(author, blog);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        context.ChangeTracker.Clear();

        var outboxMessageCount = await context.Set<OutboxMessage>()
            .CountAsync(cancellationToken);

        var persistedBlog = await context.Blogs
            .Include(savedBlog => savedBlog.Author)
            .Include(savedBlog => savedBlog.Posts)
            .SingleAsync(cancellationToken);

        var persistedPost = persistedBlog.Posts.Single();

        TutorialConsole.WriteEvidence(
            "Persistência",
            ("Author", $"{persistedBlog.Author.Name.Value} / {persistedBlog.Author.UserName.Value}"),
            ("Blog", $"{persistedBlog.Name.Value} ({persistedBlog.Url.Value})"),
            ("Post", $"{persistedPost.Title.Value} -> {persistedPost.StateName}"),
            ("Domain events capturados", capturedEvents),
            ("Outbox messages", outboxMessageCount.ToString()),
            ("Domain events após limpeza", aggregates.Sum(aggregate => aggregate.DomainEvents.Count).ToString()));
    }

    private static string FormatDomainEvents(IEnumerable<AggregateRoot> aggregates)
    {
        var eventNames = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .Select(domainEvent => domainEvent.GetType().Name)
            .ToArray();

        return eventNames.Length == 0 ? "(nenhum)" : string.Join(", ", eventNames);
    }
}
