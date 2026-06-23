namespace CSharpCodePortfolio.Tutorials.Tutorial11;

internal interface IRepository<T, TKey>
    where TKey : notnull
{
    IReadOnlyList<T> List();

    T? GetById(TKey id);

    void Add(T entity);

    bool Update(T entity);

    bool Delete(TKey id);
}
