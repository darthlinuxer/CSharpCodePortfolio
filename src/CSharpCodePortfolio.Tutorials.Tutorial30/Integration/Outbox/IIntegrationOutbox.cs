using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;

/// <summary>
/// Application port that stores integration events in the current transaction.
/// </summary>
public interface IIntegrationOutbox
{
    void Add(IIntegrationEvent integrationEvent);
}
