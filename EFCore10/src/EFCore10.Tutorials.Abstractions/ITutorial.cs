namespace EFCore10.Tutorials.Abstractions;

/// <summary>
/// Defines the executable contract for a tutorial discovered by the CLI.
/// </summary>
public interface ITutorial
{
    /// <summary>
    /// Runs the tutorial.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel tutorial execution.</param>
    public Task RunAsync(CancellationToken cancellationToken);
}
