using System.ComponentModel;
using EFCore10.App.Tutorials;
using Spectre.Console;
using Spectre.Console.Cli;

namespace EFCore10.App.Commands;

public sealed class RunCommand : AsyncCommand<RunCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id-or-slug>")]
        [Description("The tutorial id or slug to run.")]
        public string Tutorial { get; init; } = string.Empty;
    }

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var tutorials = TutorialRegistry.Discover();
        var tutorial = tutorials.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, settings.Tutorial, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.Slug, settings.Tutorial, StringComparison.OrdinalIgnoreCase));

        if (tutorial is null)
        {
            AnsiConsole.MarkupLine($"[red]Tutorial '{Markup.Escape(settings.Tutorial)}' was not found.[/]");
            AnsiConsole.Write(TutorialTable.Create(tutorials));
            return 1;
        }

        await TutorialRunner.RunAsync(
            tutorial,
            cancellationToken,
            clearBeforeRun: AnsiConsole.Profile.Capabilities.Interactive,
            pauseMessage: AnsiConsole.Profile.Capabilities.Interactive
                ? "Press any key to exit..."
                : null).ConfigureAwait(false);

        return 0;
    }
}
