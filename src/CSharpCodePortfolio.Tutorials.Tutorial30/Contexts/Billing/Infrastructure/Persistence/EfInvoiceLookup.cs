using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Infrastructure.Persistence;

public sealed class EfInvoiceLookup(Tutorial30DbContext dbContext) : IInvoiceLookup
{
    public Task<bool> ExistsForIntegrationEventAsync(Guid integrationEventId, CancellationToken cancellationToken) =>
        dbContext.Invoices.AsNoTracking()
            .AnyAsync(invoice => invoice.SourceIntegrationEventId == integrationEventId, cancellationToken);
}
