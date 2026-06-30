namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;

/// <summary>
/// Billing invoice lifecycle.
/// </summary>
public enum InvoiceStatus
{
    Open = 0,
    Paid = 1,
    Cancelled = 2
}
