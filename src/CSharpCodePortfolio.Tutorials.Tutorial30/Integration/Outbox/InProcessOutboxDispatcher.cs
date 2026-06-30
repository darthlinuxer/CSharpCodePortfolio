using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Handlers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;

/// <summary>
/// Tutorial dispatcher that proves outbox delivery without introducing a broker.
/// </summary>
public sealed class InProcessOutboxDispatcher(
    Tutorial30DbContext dbContext,
    RegisterCustomerWhenUserRegisteredHandler customerHandler,
    CreateInvoiceWhenOrderConfirmedHandler billingHandler,
    TimeProvider clock)
{
    private readonly IReadOnlyDictionary<string, Func<OutboxMessage, CancellationToken, Task<bool>>> handlers =
        new Dictionary<string, Func<OutboxMessage, CancellationToken, Task<bool>>>
        {
            [UserRegisteredIntegrationEvent.EventType] = async (message, cancellationToken) =>
                (await customerHandler
                    .HandleAsync(message.Deserialize<UserRegisteredIntegrationEvent>(), cancellationToken)
                    .ConfigureAwait(false)).IsRight,
            [OrderConfirmedIntegrationEvent.EventType] = async (message, cancellationToken) =>
                (await billingHandler
                    .HandleAsync(message.Deserialize<OrderConfirmedIntegrationEvent>(), cancellationToken)
                    .ConfigureAwait(false)).IsRight
        };

    public async Task<int> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        var pending = (await dbContext.OutboxMessages
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false))
            .Where(message => message.ProcessedAtUtc.IsNone)
            .OrderBy(message => message.OccurredAtUtc.Value)
            .ToArray();
        var dispatched = 0;

        foreach (var message in pending)
        {
            dispatched += await DispatchAsync(message, cancellationToken).ConfigureAwait(false);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return dispatched;
    }

    private async Task<int> DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var handled = await HandlerFor(message)
            .Match(
                Some: handler => handler(message, cancellationToken),
                None: () => Task.FromResult(false))
            .ConfigureAwait(false);

        return handled ? MarkProcessed(message) : 0;
    }

    private Option<Func<OutboxMessage, CancellationToken, Task<bool>>> HandlerFor(OutboxMessage message) =>
        handlers.TryGetValue(message.Type, out var handler) ? Some(handler) : None;

    private int MarkProcessed(OutboxMessage message)
    {
        message.MarkProcessed(Timestamp.UtcNow(clock));
        return 1;
    }
}
