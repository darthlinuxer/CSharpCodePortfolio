using CSharpCodePortfolio.App.Tutorials;
using Spectre.Console;

namespace CSharpCodePortfolio.App.Commands;

internal static class TutorialRunner
{
    public static async Task RunAsync(
        TutorialDescriptor tutorial,
        bool clearBeforeRun,
        string? pauseMessage,
        CancellationToken cancellationToken)
    {
        if (clearBeforeRun)
        {
            AnsiConsole.Clear();
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Running {Markup.Escape(tutorial.Id)} - {Markup.Escape(tutorial.Title)}[/]");
        AnsiConsole.WriteLine();

        await tutorial.ExecuteAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(pauseMessage))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[dim]{Markup.Escape(pauseMessage)}[/]");
            await AnsiConsole.Console.Input.ReadKeyAsync(intercept: true, cancellationToken).ConfigureAwait(false);
        }
    }
}
