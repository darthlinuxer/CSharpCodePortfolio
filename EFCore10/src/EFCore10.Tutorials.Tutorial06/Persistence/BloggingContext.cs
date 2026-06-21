using EFCore10.Tutorials.Tutorial06.Models;
using EFCore10.Tutorials.Tutorial06.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial06.Persistence;

public sealed class BloggingContext(DbContextOptions<BloggingContext> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();

    public DbSet<Blog> Blogs => Set<Blog>();

    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);

    public override int SaveChanges()
    {
        AddOutboxMessages();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessages();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddOutboxMessages()
    {
        ChangeTracker.DetectChanges();

        var domainEvents = ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<AggregateRoot>()
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToArray();

        if (domainEvents.Length == 0)
            return;

        Set<OutboxMessage>().AddRange(domainEvents.Select(OutboxMessage.FromDomainEvent));
    }
}
