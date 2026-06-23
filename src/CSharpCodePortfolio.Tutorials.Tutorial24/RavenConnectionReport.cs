namespace CSharpCodePortfolio.Tutorials.Tutorial24;

internal sealed record RavenConnectionReport(
    string Database,
    IReadOnlyList<string> HostUrls,
    IReadOnlyList<string> ContainerUrls,
    int MaxRequestsPerSession,
    bool UseOptimisticConcurrency,
    IReadOnlyList<string> SessionSteps);
