using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Messaging;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Messaging;

public interface IOutboxIntegrationEventConsumer
{
    string EventType { get; }

    Task<bool> HandleAsync(OutboxMessage message, CancellationToken cancellationToken);
}

public sealed class OutboxIntegrationEventBus(Tutorial30DbContext dbContext) : IIntegrationEventBus
{
    public Task<Either<Seq<DomainError>, Unit>> PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        dbContext.OutboxMessages.Add(OutboxMessage.From(integrationEvent));
        return Task.FromResult(Right<Seq<DomainError>, Unit>(default));
    }
}

public sealed class OutboxIntegrationEventConsumer<TIntegrationEvent, TResult>(
    string eventType,
    IIntegrationEventHandler<TIntegrationEvent, TResult> handler) : IOutboxIntegrationEventConsumer
    where TIntegrationEvent : IIntegrationEvent
{
    public string EventType => eventType;

    public async Task<bool> HandleAsync(OutboxMessage message, CancellationToken cancellationToken) =>
        (await handler
            .HandleAsync(message.Deserialize<TIntegrationEvent>(), cancellationToken)
            .ConfigureAwait(false)).IsRight;
}

public sealed class InProcessOutboxDispatcher(
    Tutorial30DbContext dbContext,
    IEnumerable<IOutboxIntegrationEventConsumer> consumers,
    TimeProvider clock)
{
    private readonly IReadOnlyDictionary<string, IOutboxIntegrationEventConsumer> consumersByEventType =
        consumers.ToDictionary(consumer => consumer.EventType);

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
        var handled = await ConsumerFor(message)
            .Match(
                Some: consumer => consumer.HandleAsync(message, cancellationToken),
                None: () => Task.FromResult(false))
            .ConfigureAwait(false);

        return handled ? MarkProcessed(message) : 0;
    }

    private Option<IOutboxIntegrationEventConsumer> ConsumerFor(OutboxMessage message) =>
        consumersByEventType.TryGetValue(message.Type, out var consumer) ? Some(consumer) : None;

    private int MarkProcessed(OutboxMessage message)
    {
        message.MarkProcessed(Timestamp.UtcNow(clock));
        return 1;
    }
}
