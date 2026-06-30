using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;

/// <summary>
/// EF Core outbox adapter used by application services before committing the unit of work.
/// </summary>
public sealed class EfIntegrationOutbox(Tutorial30DbContext dbContext) : IIntegrationOutbox
{
    public void Add(IIntegrationEvent integrationEvent)
    {
        dbContext.OutboxMessages.Add(OutboxMessage.From(integrationEvent));
    }
}
