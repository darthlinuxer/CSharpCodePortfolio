using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

public sealed partial class Tutorial30DbContext :
    IRepository<UserAccount, Guid>,
    IRepository<Order, OrderId>,
    IRepository<Invoice, InvoiceId>
{
    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>() =>
        (IRepository<TAggregate, TKey>)(object)this;

    async Task<Option<UserAccount>> IRepository<UserAccount, Guid>.FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var account = await Users
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return Optional(account);
    }

    void IRepository<UserAccount, Guid>.Add(UserAccount aggregate) =>
        Users.Add(aggregate);

    void IRepository<UserAccount, Guid>.Remove(UserAccount aggregate) =>
        Users.Remove(aggregate);

    async Task<Option<Order>> IRepository<Order, OrderId>.FindByIdAsync(
        OrderId id,
        CancellationToken cancellationToken)
    {
        var order = await Orders
            .Include(order => order.Lines)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return Optional(order);
    }

    void IRepository<Order, OrderId>.Add(Order aggregate) =>
        Orders.Add(aggregate);

    void IRepository<Order, OrderId>.Remove(Order aggregate) =>
        Orders.Remove(aggregate);

    async Task<Option<Invoice>> IRepository<Invoice, InvoiceId>.FindByIdAsync(
        InvoiceId id,
        CancellationToken cancellationToken)
    {
        var invoice = await Invoices
            .FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return Optional(invoice);
    }

    void IRepository<Invoice, InvoiceId>.Add(Invoice aggregate) =>
        Invoices.Add(aggregate);

    void IRepository<Invoice, InvoiceId>.Remove(Invoice aggregate) =>
        Invoices.Remove(aggregate);
}
