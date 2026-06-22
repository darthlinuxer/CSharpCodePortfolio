namespace CSharpCodePortfolio.Tutorials.Abstractions;

/// <summary>
/// Defines an executable tutorial discovered by the portfolio host.
/// </summary>
public interface ITutorial
{
    /// <summary>
    /// Runs the tutorial.
    /// </summary>
    Task RunAsync(CancellationToken cancellationToken);
}
