using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

/// <summary>
/// EF Core context used by the tutorial to prove bounded contexts and outbox in one process.
/// </summary>
public sealed class Tutorial30DbContext(DbContextOptions<Tutorial30DbContext> options)
    : DbContext(options), ITutorial30UnitOfWork
{
    /// <summary>
    /// Gets the user table used by the registration sample.
    /// </summary>
    public DbSet<UserAccount> Users => Set<UserAccount>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<CustomerDirectoryEntry> CustomerDirectory => Set<CustomerDirectoryEntry>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Commits through the application port and translates expected unique-key conflicts to typed errors.
    /// </summary>
    public async Task<Either<Seq<DomainError>, int>> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Right<Seq<DomainError>, int>(result);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            var errors = await DetectUniqueConstraintErrorsAsync(cancellationToken).ConfigureAwait(false);
            DetachFailedInserts();

            return Left<Seq<DomainError>, int>(errors);
        }
    }

    /// <summary>
    /// Applies tutorial mappings that keep persistence concerns outside the aggregate.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Tutorial30DbContext).Assembly);

    /// <summary>
    /// Commits tracked changes and clears domain events only after EF Core persists successfully.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = ChangeTracker.Entries()
            .Select(entry => entry.Entity)
            .OfType<IAggregate>()
            .Where(aggregate => aggregate.HasDomainEvents)
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        aggregatesWithEvents.Iter(aggregate => aggregate.ClearDomainEvents());

        return result;
    }

    /// <summary>
    /// Detects provider messages that represent unique index violations.
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Narrows a unique-key exception back to typed user registration errors.
    /// </summary>
    private async Task<Seq<DomainError>> DetectUniqueConstraintErrorsAsync(CancellationToken cancellationToken)
    {
        var accounts = ChangeTracker
            .Entries<UserAccount>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .ToArray();

        var errors = new List<DomainError>();

        foreach (var account in accounts)
        {
            _ = (await Users.AsNoTracking()
                    .AnyAsync(user => user.Id != account.Id && user.Email == account.Email, cancellationToken)
                    .ConfigureAwait(false))
                ? Add(errors, new UserAccountEmailDuplicateError())
                : default;
        }

        return errors.Count == 0
            ? Seq1<DomainError>(new UserAccountRegistrationConflictError())
            : errors.DistinctBy(error => error.GetType()).ToSeq();
    }

    /// <summary>
    /// Removes failed inserts from tracking so a returned Left does not poison the current context.
    /// </summary>
    private void DetachFailedInserts()
    {
        foreach (var entry in ChangeTracker.Entries<UserAccount>().Where(entry => entry.State == EntityState.Added))
        {
            entry.State = EntityState.Detached;
        }
    }

    private static Unit Add(List<DomainError> errors, DomainError error)
    {
        errors.Add(error);
        return default;
    }
}
