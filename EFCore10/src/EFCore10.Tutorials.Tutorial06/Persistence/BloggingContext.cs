using EFCore10.Tutorials.Tutorial06.Models;
using EFCore10.Tutorials.Tutorial06.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial06.Persistence;

public sealed class BloggingContext(DbContextOptions<BloggingContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Blog> Blogs => Set<Blog>();

    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);

    public override int SaveChanges()
    {
        var aggregates = AddOutboxMessages();
        var result = base.SaveChanges();
        ClearDomainEvents(aggregates);

        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = AddOutboxMessages();
        var result = await base.SaveChangesAsync(cancellationToken);
        ClearDomainEvents(aggregates);

        return result;
    }

    private AggregateRoot[] AddOutboxMessages()
    {
        ChangeTracker.DetectChanges();

        var aggregates = ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<AggregateRoot>()
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToArray();

        var domainEvents = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToArray();

        if (domainEvents.Length == 0)
            return [];

        Set<OutboxMessage>().AddRange(domainEvents.Select(OutboxMessage.FromDomainEvent));

        return aggregates;
    }

    private static void ClearDomainEvents(IEnumerable<AggregateRoot> aggregates)
    {
        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();
    }
}
