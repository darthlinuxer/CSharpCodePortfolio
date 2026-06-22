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
        var author = Tutorial06SampleData.InviteAcceptedAuthor(blog, owner, authorUser);
        var post = Tutorial06SampleData.AddArchivedPost(blog, authorUser);
        blog.Rename(BlogName.Create("EF Core Ownership Notes"), owner);
        blog.TransferOwnership(successorOwner, owner);

        var aggregates = new AggregateRoot[] { owner, authorUser, successorOwner, blog, post };
        var capturedEvents = Tutorial06EvidenceFormatter.FormatDomainEvents(aggregates);

        context.AddRange(owner, authorUser, successorOwner, blog);
        await context.SaveChangesAsync(cancellationToken);

        context.ChangeTracker.Clear();

        var outboxMessages = await context.Set<OutboxMessage>()
            .AsNoTracking()
            .OrderBy(message => message.OccurredOnUtc)
            .ToListAsync(cancellationToken);

        var persistedBlog = await context.Blogs
            .Include(savedBlog => savedBlog.Memberships)
                .ThenInclude(savedMembership => savedMembership.User)
            .Include(savedBlog => savedBlog.Posts)
                .ThenInclude(savedPost => savedPost.PostedBy)
            .AsSplitQuery()
            .SingleAsync(cancellationToken);

        var persistedPost = persistedBlog.Posts.Single();
        var persistedAuthor = persistedBlog.Authors.Single(savedAuthor => savedAuthor.Id == author.Id);
        var publishedOnUtc = persistedPost.PublishedOnUtc.GetValueOrDefault();
        var publishedFrom = publishedOnUtc.Add(TimeSpan.FromDays(-1));
        var publishedTo = publishedOnUtc.Add(TimeSpan.FromDays(1));
        var postsInPublishedRange = await context.Posts
            .AsNoTracking()
            .Where(savedPost => savedPost.BlogId == persistedBlog.Id)
            .Where(savedPost => savedPost.PublishedOnUtc >= publishedFrom && savedPost.PublishedOnUtc <= publishedTo)
            .CountAsync(cancellationToken);

        var sampleOutboxMessage = outboxMessages.First(message => message.EventName == "blog.created");

        TutorialConsole.WriteEvidence(
            "Persistência",
            ("Owner atual", persistedBlog.CurrentOwner.User.UserName.Value),
            ("Author membership", $"{persistedAuthor.User.Name.Value} / {persistedAuthor.RoleName} / {persistedAuthor.StateName}"),
            ("Blog", $"{persistedBlog.Name.Value} ({persistedBlog.Url.Value})"),
            ("Post", $"{persistedPost.Title.Value} por {persistedPost.PostedBy.UserName.Value} -> {persistedPost.StateName}"),
            ("Estados do workflow", Tutorial06EvidenceFormatter.FormatWorkflowStates(persistedBlog, persistedAuthor, persistedPost)),
            ("Publicado em", persistedPost.PublishedOnUtc?.ToString() ?? "(sem publicação)"),
            ("Posts no range", postsInPublishedRange.ToString()),
            ("Domain events capturados", capturedEvents),
            ("Outbox messages", outboxMessages.Count.ToString()),
            ("Outbox eventos", Tutorial06EvidenceFormatter.FormatOutboxEventNames(outboxMessages)),
            ("Status da outbox", Tutorial06EvidenceFormatter.FormatOutboxDispatchStatus(outboxMessages)),
            ("Outbox payload", $"{sampleOutboxMessage.EventName} -> {sampleOutboxMessage.Payload}"),
            ("Domain events após limpeza", aggregates.Sum(aggregate => aggregate.DomainEvents.Count).ToString()));
    }
}
