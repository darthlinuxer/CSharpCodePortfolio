using System.Net;
using System.Text;

namespace CSharpCodePortfolio.Tutorials.Tutorial20;

internal sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
{
    public int CallCount { get; private set; }

    public RecordedHttpRequest? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        LastRequest = new RecordedHttpRequest(
            request.Method,
            request.RequestUri ?? throw new InvalidOperationException("A requisição deve ter URI."));

        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
            RequestMessage = request
        };

        return Task.FromResult(response);
    }
}
