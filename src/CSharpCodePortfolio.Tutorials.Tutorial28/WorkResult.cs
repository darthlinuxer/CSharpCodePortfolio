namespace CSharpCodePortfolio.Tutorials.Tutorial28;

internal sealed record WorkResult(
    string Name,
    int Score,
    int ObservedConcurrency,
    int ManagedThreadId);
