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

        var owner = Tutorial06SampleData.CreateDemoOwner();
        var authorUser = Tutorial06SampleData.CreateDemoAuthor();
        var successorOwner = Tutorial06SampleData.CreateDemoSuccessorOwner();
        var blog = Tutorial06SampleData.CreateDemoBlog(owner);
        var author = Tutorial06SampleData.InviteAcceptedAuthor(blog, authorUser);
        var post = Tutorial06SampleData.AddArchivedPost(blog, authorUser);
        blog.Rename(BlogName.Create("EF Core Ownership Notes"));
        blog.TransferOwnership(successorOwner);

        var aggregates = new AggregateRoot[] { owner, authorUser, successorOwner, blog, post };
        var capturedEvents = FormatDomainEvents(aggregates);

        context.AddRange(owner, authorUser, successorOwner, blog);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        context.ChangeTracker.Clear();

        var outboxMessageCount = await context.Set<OutboxMessage>()
            .CountAsync(cancellationToken);

        var persistedBlog = await context.Blogs
            .Include(savedBlog => savedBlog.Owners)
                .ThenInclude(savedOwner => savedOwner.User)
            .Include(savedBlog => savedBlog.Authors)
                .ThenInclude(savedAuthor => savedAuthor.User)
            .Include(savedBlog => savedBlog.Posts)
                .ThenInclude(savedPost => savedPost.PostedBy)
            .AsSplitQuery()
            .SingleAsync(cancellationToken);

        var persistedPost = persistedBlog.Posts.Single();
        var persistedAuthor = persistedBlog.Authors.Single(savedAuthor => savedAuthor.Id == author.Id);

        TutorialConsole.WriteEvidence(
            "Persistência",
            ("Owner atual", persistedBlog.CurrentOwner.User.UserName.Value),
            ("Author convidado", $"{persistedAuthor.User.Name.Value} / {persistedAuthor.StateName}"),
            ("Blog", $"{persistedBlog.Name.Value} ({persistedBlog.Url.Value})"),
            ("Post", $"{persistedPost.Title.Value} por {persistedPost.PostedBy.UserName.Value} -> {persistedPost.StateName}"),
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
