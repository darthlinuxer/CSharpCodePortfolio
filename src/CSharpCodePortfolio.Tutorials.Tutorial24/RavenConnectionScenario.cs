namespace CSharpCodePortfolio.Tutorials.Tutorial24;

internal static class RavenConnectionScenario
{
    public static RavenConnectionReport Run()
    {
        var settings = RavenDatabaseSettings.CreateDefault();
        using var store = RavenDocumentStoreFactory.CreateConfiguredStore(settings, runningInContainer: false);
        var sessionSteps = RavenSessionFlow.DescribeUserFlow(
            new RavenUser("users/1-A", "Ada", "Lovelace", "London", "United Kingdom"),
            page: 1,
            pageSize: 10);

        return new RavenConnectionReport(
            settings.Database,
            RavenDocumentStoreFactory.SelectUrls(settings, runningInContainer: false),
            RavenDocumentStoreFactory.SelectUrls(settings, runningInContainer: true),
            store.Conventions.MaxNumberOfRequestsPerSession,
            store.Conventions.UseOptimisticConcurrency,
            sessionSteps);
    }
}
