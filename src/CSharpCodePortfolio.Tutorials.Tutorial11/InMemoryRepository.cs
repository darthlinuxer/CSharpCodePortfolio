namespace CSharpCodePortfolio.Tutorials.Tutorial11;

internal sealed class InMemoryRepository<T, TKey>(Func<T, TKey> keySelector) : IRepository<T, TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, T> entities = [];

    public IReadOnlyList<T> List() =>
        entities.Values.ToArray();

    public T? GetById(TKey id) =>
        entities.TryGetValue(id, out var entity) ? entity : default;

    public void Add(T entity) =>
        entities.Add(keySelector(entity), entity);

    public bool Update(T entity)
    {
        var id = keySelector(entity);
        if (!entities.ContainsKey(id))
        {
            return false;
        }

        entities[id] = entity;
        return true;
    }

    public bool Delete(TKey id) =>
        entities.Remove(id);
}
