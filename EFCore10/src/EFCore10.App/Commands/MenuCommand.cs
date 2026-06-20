using EFCore10.App.Tutorials;
using Spectre.Console;
using Spectre.Console.Cli;

namespace EFCore10.App.Commands;

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
            AnsiConsole.MarkupLine("[yellow]No tutorials were discovered.[/]");
            return 1;
        }

        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine("[yellow]The interactive menu requires an interactive terminal. Use 'list' or 'run <id-or-slug>'.[/]");
            AnsiConsole.Write(TutorialTable.Create(tutorials));
            return 1;
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
                    .Title("Select a tutorial to run")
                    .PageSize(10)
                    .AddChoices(choices),
                cancellationToken).ConfigureAwait(false);

            if (selected.Tutorial is null)
            {
                return 0;
            }

            await TutorialRunner.RunAsync(
                selected.Tutorial,
                cancellationToken,
                clearBeforeRun: true,
                pauseMessage: "Press any key to return to the menu...").ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return 1;
    }

    private sealed record TutorialMenuItem(string Display, TutorialDescriptor? Tutorial)
    {
        public static TutorialMenuItem Exit { get; } = new("Exit", null);

        public static TutorialMenuItem FromTutorial(TutorialDescriptor tutorial)
        {
            return new TutorialMenuItem($"{tutorial.Id} - {tutorial.Title}", tutorial);
        }

        public override string ToString()
        {
            return Display;
        }
    }
}
