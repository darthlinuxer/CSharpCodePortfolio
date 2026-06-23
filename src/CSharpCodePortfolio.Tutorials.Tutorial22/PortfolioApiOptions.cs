namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed class PortfolioApiOptions
{
    public string ApiUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    public int RetryCount { get; init; }
}
