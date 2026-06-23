using System.Linq.Expressions;

namespace CSharpCodePortfolio.Tutorials.Tutorial14;

internal interface IRepository<T, TKey>
    where T : class
    where TKey : notnull
{
    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken);

    Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken);

    void Add(T entity);

    void Update(T entity);

    void Delete(T entity);
}
