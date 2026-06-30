using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Messaging;

public interface IIntegrationEventBus
{
    Task<Either<Seq<DomainError>, Unit>> PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}

public interface IIntegrationEventHandler<in TIntegrationEvent, TResult>
    where TIntegrationEvent : IIntegrationEvent
{
    Task<Either<Seq<DomainError>, TResult>> HandleAsync(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
