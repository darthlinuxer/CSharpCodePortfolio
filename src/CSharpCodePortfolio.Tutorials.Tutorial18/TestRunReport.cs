namespace CSharpCodePortfolio.Tutorials.Tutorial18;

internal sealed record TestRunReport(
    string TestClassName,
    int DiscoveredTests,
    int PassedTests,
    IReadOnlyList<ReflectedTestCase> Cases);
