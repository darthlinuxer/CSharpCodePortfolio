using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Messaging;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Handlers;

public sealed class PublishUserRegisteredIntegrationEventHandler(IIntegrationEventBus integrationEventBus)
    : IDomainEventHandler<UserAccountRegisteredDomainEvent>
{
    public Task<Either<Seq<DomainError>, Unit>> HandleAsync(
        UserAccountRegisteredDomainEvent domainEvent,
        CancellationToken cancellationToken) =>
        integrationEventBus.PublishAsync(new UserRegisteredIntegrationEvent(
                Guid.CreateVersion7(),
                domainEvent.OccurredAtUtc,
                domainEvent.UserId,
                domainEvent.Email.Value),
            cancellationToken);
}
