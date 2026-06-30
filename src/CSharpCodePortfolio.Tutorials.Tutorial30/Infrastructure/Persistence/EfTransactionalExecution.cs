using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

public sealed class EfTransactionalExecution(Tutorial30DbContext dbContext) : ITransactionalExecution
{
    public async Task<Either<Seq<DomainError>, TResult>> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<Either<Seq<DomainError>, TResult>>> work,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(work);

        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            work,
            async (_, state, token) => await ExecuteInTransactionAsync(state, token).ConfigureAwait(false),
            null,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, TResult>> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<Either<Seq<DomainError>, TResult>>> work,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = await work(cancellationToken).ConfigureAwait(false);

        return await result.Match(
            Right: async value =>
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return Right<Seq<DomainError>, TResult>(value);
            },
            Left: async errors =>
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return Left<Seq<DomainError>, TResult>(errors);
            })
            .ConfigureAwait(false);
    }
}
