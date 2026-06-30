namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Persistence;

public interface IInvoiceLookup
{
    Task<bool> ExistsForIntegrationEventAsync(Guid integrationEventId, CancellationToken cancellationToken);
}
