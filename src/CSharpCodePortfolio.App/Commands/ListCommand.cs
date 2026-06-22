using CSharpCodePortfolio.App.Tutorials;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CSharpCodePortfolio.App.Commands;

public sealed class ListCommand : AsyncCommand<ListCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    protected override Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var tutorials = TutorialRegistry.Discover();

        if (tutorials.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No executable tutorials were discovered.[/]");
            return Task.FromResult(0);
        }

        AnsiConsole.Write(TutorialTable.Create(tutorials));
        return Task.FromResult(0);
    }
}
