using CSharpCodePortfolio.App.Tutorials;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CSharpCodePortfolio.App.Commands;

public sealed class MenuCommand : AsyncCommand<MenuCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var tutorials = TutorialRegistry.Discover();

        if (tutorials.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No executable tutorials were discovered.[/]");
            return 0;
        }

        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine("[yellow]The interactive menu requires a terminal. Use 'list' or 'run <id-or-slug>'.[/]");
            AnsiConsole.Write(TutorialTable.Create(tutorials));
            return 0;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(TutorialTable.Create(tutorials));

            var choices = tutorials
                .Select(TutorialMenuItem.FromTutorial)
                .Append(TutorialMenuItem.Exit)
                .ToArray();

            var selected = await AnsiConsole.PromptAsync(
                new SelectionPrompt<TutorialMenuItem>()
                    .Title("Select a tutorial group")
                    .AddChoices(choices)
                    .UseConverter(item => item.Display),
                cancellationToken).ConfigureAwait(false);

            if (selected.Tutorial is null)
            {
                return 0;
            }

            await TutorialRunner.RunAsync(
                selected.Tutorial,
                clearBeforeRun: true,
                pauseMessage: "Press any key to return to the portfolio menu...",
                cancellationToken).ConfigureAwait(false);
        }

        return 0;
    }

    private sealed record TutorialMenuItem(string Display, TutorialDescriptor? Tutorial)
    {
        public static TutorialMenuItem Exit { get; } = new("Exit", null);

        public static TutorialMenuItem FromTutorial(TutorialDescriptor tutorial)
        {
            return new TutorialMenuItem($"{tutorial.Id} - {tutorial.Title}", tutorial);
        }
    }
}
