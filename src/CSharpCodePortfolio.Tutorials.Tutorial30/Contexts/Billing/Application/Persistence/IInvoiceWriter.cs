using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Persistence;

/// <summary>
/// Command-side persistence port for Billing invoices.
/// </summary>
public interface IInvoiceWriter
{
    void Add(Invoice invoice);

    Task<bool> ExistsForIntegrationEventAsync(Guid integrationEventId, CancellationToken cancellationToken);
}
