namespace CSharpCodePortfolio.Tutorials.Tutorial24;

internal sealed record RavenDatabaseSettings(
    IReadOnlyList<string> UrlsFromHost,
    IReadOnlyList<string> UrlsFromContainer,
    string Database)
{
    public static RavenDatabaseSettings CreateDefault()
    {
        return new RavenDatabaseSettings(
            ["http://localhost:9900", "http://localhost:8080"],
            ["http://raven_A:8080", "http://raven_B:8080"],
            "UserDatabase");
    }
}
