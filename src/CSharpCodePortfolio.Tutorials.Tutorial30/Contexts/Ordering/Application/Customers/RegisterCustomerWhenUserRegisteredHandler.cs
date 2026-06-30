using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Messaging;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Persistence;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Customers;

/// <summary>
/// ACL that translates Identity's published user event into Ordering's CustomerDirectory.
/// </summary>
public sealed class RegisterCustomerWhenUserRegisteredHandler(
    ICustomerDirectory customerDirectory,
    ITutorial30UnitOfWork unitOfWork) : IIntegrationEventHandler<UserRegisteredIntegrationEvent, Unit>
{
    public async Task<Either<Seq<DomainError>, Unit>> HandleAsync(
        UserRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Create(integrationEvent.UserAccountId);

        return await customerId.Match(
            Right: id => RegisterAsync(id, integrationEvent.Email, cancellationToken),
            Left: error => Task.FromResult(Left<Seq<DomainError>, Unit>(Seq1(error)))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, Unit>> RegisterAsync(
        CustomerId customerId,
        string email,
        CancellationToken cancellationToken)
    {
        await customerDirectory.AddIfMissingAsync(customerId, email, cancellationToken).ConfigureAwait(false);
        var commit = await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return commit.Match(
            Right: _ => Right<Seq<DomainError>, Unit>(default),
            Left: errors => Left<Seq<DomainError>, Unit>(errors));
    }
}
