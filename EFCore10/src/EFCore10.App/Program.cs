using EFCore10.App.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

const int CancellationExitCode = 130;

using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
    AnsiConsole.MarkupLine("[yellow] Cancellation requested.[/]");
};

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("efcore10");
    config.SetApplicationVersion("0.1.0");
    config.CancellationExitCode(CancellationExitCode);

    config.AddCommand<MenuCommand>("menu")
        .WithDescription("Shows an interactive tutorial menu.")
        .WithExample("menu");

    config.AddCommand<ListCommand>("list")
        .WithDescription("Lists all discovered tutorials.")
        .WithExample("list");

    config.AddCommand<RunCommand>("run")
        .WithDescription("Runs a tutorial by id or slug.")
        .WithExample("run", "01")
        .WithExample("run", "simple-modeling");

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

var effectiveArgs = args.Length == 0 ? new[] { "menu" } : args;

try
{
    return await app.RunAsync(effectiveArgs, cancellationTokenSource.Token);
}
catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
{
    return CancellationExitCode;
}
