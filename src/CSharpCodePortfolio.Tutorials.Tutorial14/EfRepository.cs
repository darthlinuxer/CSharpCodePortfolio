using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial14;

internal sealed class EfRepository<T, TKey>(DbContext context) : IRepository<T, TKey>
    where T : class
    where TKey : notnull
{
    private readonly DbSet<T> dbSet = context.Set<T>();

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken) =>
        await dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken)
    {
        var query = dbSet.AsNoTracking().Where(predicate);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken) =>
        await dbSet.FindAsync([id], cancellationToken);

    public void Add(T entity) =>
        dbSet.Add(entity);

    public void Update(T entity) =>
        dbSet.Update(entity);

    public void Delete(T entity) =>
        dbSet.Remove(entity);
}
