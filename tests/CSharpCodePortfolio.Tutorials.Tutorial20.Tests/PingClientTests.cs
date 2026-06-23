using System.Net;
using CSharpCodePortfolio.Tutorials.Tutorial20;

namespace CSharpCodePortfolio.Tutorials.Tutorial20.Tests;

[TestClass]
public sealed class PingClientTests
{
    [TestMethod]
    public async Task CheckAsync_UsesConfiguredHandler()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "true");
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://portfolio.local/")
        };

        var client = new PingClient(httpClient);

        var result = await client.CheckAsync(CancellationToken.None);

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.AreEqual("true", result.Body);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, handler.CallCount);
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest.Method);
        Assert.AreEqual("https://portfolio.local/ping", handler.LastRequest.Uri.ToString());
    }
}
