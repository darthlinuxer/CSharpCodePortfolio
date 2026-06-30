using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Messaging;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Handlers;

public sealed class PublishOrderConfirmedIntegrationEventHandler(IIntegrationEventBus integrationEventBus)
    : IDomainEventHandler<OrderConfirmedDomainEvent>
{
    public Task<Either<Seq<DomainError>, Unit>> HandleAsync(
        OrderConfirmedDomainEvent domainEvent,
        CancellationToken cancellationToken) =>
        integrationEventBus.PublishAsync(new OrderConfirmedIntegrationEvent(
                Guid.CreateVersion7(),
                domainEvent.OccurredAtUtc,
                domainEvent.OrderId.Value,
                domainEvent.CustomerId.Value,
                domainEvent.Total.Value),
            cancellationToken);
}
