using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;

public interface IRepository<TAggregate, TKey>
{
    Task<Option<TAggregate>> FindByIdAsync(TKey id, CancellationToken cancellationToken);

    void Add(TAggregate aggregate);

    void Remove(TAggregate aggregate);
}

public interface IUnitOfWork
{
    Task<Either<Seq<DomainError>, PersistenceResult>> SaveEntitiesAsync(CancellationToken cancellationToken);
}

public interface ITransactionalExecution
{
    Task<Either<Seq<DomainError>, TResult>> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<Either<Seq<DomainError>, TResult>>> work,
        CancellationToken cancellationToken);
}

public readonly record struct PersistenceResult(int RowsAffected);
