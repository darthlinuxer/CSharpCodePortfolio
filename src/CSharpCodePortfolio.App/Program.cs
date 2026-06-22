using CSharpCodePortfolio.App.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

const int CancellationExitCode = 130;

using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
    AnsiConsole.MarkupLine("[yellow]Cancellation requested.[/]");
};

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("portfolio");
    config.SetApplicationVersion("0.1.0");
    config.CancellationExitCode(CancellationExitCode);

    config.AddCommand<MenuCommand>("menu")
        .WithDescription("Shows the interactive tutorial menu.")
        .WithExample("menu");

    config.AddCommand<ListCommand>("list")
        .WithDescription("Lists executable tutorial groups.")
        .WithExample("list");

    config.AddCommand<RunCommand>("run")
        .WithDescription("Runs a tutorial group by id or slug.")
        .WithExample("run", "efcore10");

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

var effectiveArgs = args.Length == 0 ? ["menu"] : args;

try
{
    return await app.RunAsync(effectiveArgs, cancellationTokenSource.Token).ConfigureAwait(false);
}
catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
{
    return CancellationExitCode;
}
