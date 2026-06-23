using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed class VerseClient(HttpClient httpClient, IOptions<PortfolioApiOptions> options)
{
    public async Task<VerseResponse> GetAsync(string reference, CancellationToken cancellationToken)
    {
        var uri = new Uri(new Uri(options.Value.ApiUrl), Uri.EscapeDataString(reference));
        return await httpClient.GetFromJsonAsync<VerseResponse>(uri, cancellationToken)
            ?? throw new InvalidOperationException("A resposta do cliente HTTP deve conter um verso.");
    }
}
