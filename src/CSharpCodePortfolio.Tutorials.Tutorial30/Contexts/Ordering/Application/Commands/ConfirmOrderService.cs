using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Commands;

/// <summary>
/// Confirms an order and lets the domain event flow publish the Billing contract.
/// </summary>
public sealed class ConfirmOrderService(
    IRepository<Order, OrderId> repository,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<Either<Seq<DomainError>, ConfirmedOrderDto>> ConfirmAsync(
        ConfirmOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await OrderId.Create(request.OrderId).Match(
            Right: id => ConfirmExistingAsync(id, cancellationToken),
            Left: error => Task.FromResult(Left<Seq<DomainError>, ConfirmedOrderDto>(Seq1(error)))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, ConfirmedOrderDto>> ConfirmExistingAsync(
        OrderId orderId,
        CancellationToken cancellationToken)
    {
        var order = await repository.FindByIdAsync(orderId, cancellationToken).ConfigureAwait(false);

        return await order.Match(
            Some: value => ConfirmLoadedAsync(value, cancellationToken),
            None: () => Task.FromResult(Left<Seq<DomainError>, ConfirmedOrderDto>(Seq1<DomainError>(new OrderNotFoundError()))))
            .ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, ConfirmedOrderDto>> ConfirmLoadedAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        var confirmed = order.Confirm(clock);

        return await confirmed.Match(
            Right: _ => SaveAsync(order, cancellationToken),
            Left: error => Task.FromResult(Left<Seq<DomainError>, ConfirmedOrderDto>(Seq1(error)))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, ConfirmedOrderDto>> SaveAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        var saved = await unitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

        return saved.Match(
            Right: _ => Right<Seq<DomainError>, ConfirmedOrderDto>(new ConfirmedOrderDto(
                order.Id.Value,
                order.CustomerId.Value,
                order.Total.Value)),
            Left: errors => Left<Seq<DomainError>, ConfirmedOrderDto>(errors));
    }
}
