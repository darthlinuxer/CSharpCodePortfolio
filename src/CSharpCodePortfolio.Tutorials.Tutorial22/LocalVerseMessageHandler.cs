using System.Net;
using System.Net.Http.Json;

namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed class LocalVerseMessageHandler(PipelineTrace trace) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        trace.Record("httpclient:verso");

        var reference = Uri.UnescapeDataString(request.RequestUri?.Segments.LastOrDefault()?.TrimEnd('/') ?? "sem-referencia");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new VerseResponse(reference, "No princípio era o Verbo."))
        };

        return Task.FromResult(response);
    }
}
