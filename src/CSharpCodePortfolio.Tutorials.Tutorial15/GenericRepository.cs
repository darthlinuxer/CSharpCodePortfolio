namespace CSharpCodePortfolio.Tutorials.Tutorial15;

internal sealed class GenericRepository<T>
{
    public string Add(T entity) =>
        $"Adicionando objeto do tipo {typeof(T).Name}: {entity}";
}
