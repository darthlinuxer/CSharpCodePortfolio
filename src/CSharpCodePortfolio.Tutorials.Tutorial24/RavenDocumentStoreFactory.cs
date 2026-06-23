using Raven.Client.Documents;

namespace CSharpCodePortfolio.Tutorials.Tutorial24;

internal static class RavenDocumentStoreFactory
{
    public static IReadOnlyList<string> SelectUrls(RavenDatabaseSettings settings, bool runningInContainer)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var urls = runningInContainer ? settings.UrlsFromContainer : settings.UrlsFromHost;
        if (urls.Count == 0)
        {
            throw new InvalidOperationException("A configuração do RavenDB deve informar pelo menos uma URL.");
        }

        return urls;
    }

    public static DocumentStore CreateConfiguredStore(RavenDatabaseSettings settings, bool runningInContainer)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Database);

        var urls = SelectUrls(settings, runningInContainer);

        // ponytail: sem Initialize aqui; o teste valida a configuração sem exigir um servidor RavenDB.
        return new DocumentStore
        {
            Urls = urls.ToArray(),
            Database = settings.Database,
            Conventions =
            {
                MaxNumberOfRequestsPerSession = 10,
                UseOptimisticConcurrency = true
            }
        };
    }
}
