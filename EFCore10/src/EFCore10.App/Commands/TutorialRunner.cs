using EFCore10.App.Tutorials;
using Spectre.Console;

namespace EFCore10.App.Commands;

internal static class TutorialRunner
{
    public static async Task RunAsync(
        TutorialDescriptor tutorial,
        CancellationToken cancellationToken,
        bool clearBeforeRun,
        string? pauseMessage)
    {
        if (clearBeforeRun)
        {
            AnsiConsole.Clear();
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Running {Markup.Escape(tutorial.Id)} - {Markup.Escape(tutorial.Title)}[/]");
        AnsiConsole.WriteLine();

        await tutorial.RunAsync(cancellationToken).ConfigureAwait(false);

        AnsiConsole.WriteLine();

        if (!string.IsNullOrWhiteSpace(pauseMessage) && !cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.MarkupLine($"[dim]{Markup.Escape(pauseMessage)}[/]");
            await AnsiConsole.Console.Input.ReadKeyAsync(intercept: true, cancellationToken).ConfigureAwait(false);
        }
    }
}
