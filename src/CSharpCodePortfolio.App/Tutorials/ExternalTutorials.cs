using CSharpCodePortfolio.App.Infrastructure;
using Spectre.Console;

namespace CSharpCodePortfolio.App.Tutorials;

internal static class ExternalTutorials
{
    public static TutorialDescriptor EfCore10()
    {
        return new TutorialDescriptor(
            "efcore10",
            "efcore10",
            "Exemplos de EF Core 10",
            RunEfCore10Async);
    }

    private static Task RunEfCore10Async(CancellationToken cancellationToken)
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var projectPath = Path.Combine(repositoryRoot, "EFCore10", "src", "EFCore10.App", "EFCore10.App.csproj");

        if (!File.Exists(projectPath))
        {
            throw new FileNotFoundException("The EFCore10 tutorial app was not found.", projectPath);
        }

        var command = AnsiConsole.Profile.Capabilities.Interactive ? "menu" : "list";
        return DotNetProjectRunner.RunAsync(projectPath, command, cancellationToken);
    }
}
