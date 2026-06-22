namespace CSharpCodePortfolio.App.Infrastructure;

internal static class RepositoryPaths
{
    public static string FindRepositoryRoot()
    {
        return FindRepositoryRoot(Directory.GetCurrentDirectory())
            ?? FindRepositoryRoot(AppContext.BaseDirectory)
            ?? throw new DirectoryNotFoundException("Could not find the CSharpCodePortfolio repository root.");
    }

    private static string? FindRepositoryRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);

        while (current is not null)
        {
            var hasRootSolution = File.Exists(Path.Combine(current.FullName, "CSharpCodePortfolio.slnx"));
            var hasEfCore10 = File.Exists(Path.Combine(current.FullName, "EFCore10", "EFCore10.slnx"));

            if (hasRootSolution && hasEfCore10)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}
