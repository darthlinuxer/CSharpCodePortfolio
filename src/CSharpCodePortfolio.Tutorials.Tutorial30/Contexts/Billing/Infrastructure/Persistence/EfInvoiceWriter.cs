using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Infrastructure.Persistence;

/// <summary>
/// EF Core adapter for Billing invoice persistence.
/// </summary>
public sealed class EfInvoiceWriter(Tutorial30DbContext dbContext) : IInvoiceWriter
{
    public void Add(Invoice invoice)
    {
        dbContext.Invoices.Add(invoice);
    }

    public Task<bool> ExistsForIntegrationEventAsync(Guid integrationEventId, CancellationToken cancellationToken) =>
        dbContext.Invoices.AsNoTracking()
            .AnyAsync(invoice => invoice.SourceIntegrationEventId == integrationEventId, cancellationToken);
}
