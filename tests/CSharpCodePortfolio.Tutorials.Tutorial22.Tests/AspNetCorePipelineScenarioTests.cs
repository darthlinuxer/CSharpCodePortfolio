using System.Net;
using CSharpCodePortfolio.Tutorials.Tutorial22;

namespace CSharpCodePortfolio.Tutorials.Tutorial22.Tests;

[TestClass]
public sealed class AspNetCorePipelineScenarioTests
{
    [TestMethod]
    public async Task RunAsync_ExecutesAspNetCorePipeline()
    {
        var report = await new AspNetCorePipelineScenario().RunAsync(CancellationToken.None);

        Assert.StartsWith("http://127.0.0.1:", report.BaseAddress);
        Assert.AreEqual("https://portfolio.local/verses/", report.ApiUrl);
        Assert.AreEqual("local-key", report.ApiKey);
        Assert.AreEqual(3, report.RetryCount);
        Assert.AreEqual("GetPortfolioEmployee", report.EndpointName);
        CollectionAssert.Contains(report.EndpointTags.ToArray(), "Employees");

        Assert.AreEqual(HttpStatusCode.OK, report.EmployeeRequest.StatusCode);
        Assert.AreEqual(7, report.EmployeeRequest.Employee?.Id);
        Assert.AreEqual("Ada Lovelace", report.EmployeeRequest.Employee?.Name);
        Assert.AreEqual("John 1:1", report.EmployeeRequest.Employee?.VerseReference);
        Assert.AreEqual("No princípio era o Verbo.", report.EmployeeRequest.Employee?.VerseText);
        CollectionAssert.AreEqual(
            new[]
            {
                "middleware:classe:antes",
                "middleware:inline:antes",
                "filter:endpoint:antes",
                "handler:consulta",
                "httpclient:verso",
                "filter:endpoint:depois",
                "middleware:inline:depois",
                "middleware:classe:depois"
            },
            report.EmployeeRequest.Trace.ToArray());

        Assert.AreEqual(HttpStatusCode.OK, report.CachedRequest.StatusCode);
        Assert.AreEqual("Resposta em cache", report.CachedRequest.Employee?.Name);
        Assert.AreEqual("Resposta entregue pela ramificação.", report.CachedRequest.Employee?.VerseText);
        CollectionAssert.AreEqual(
            new[]
            {
                "middleware:classe:antes",
                "middleware:inline:antes",
                "ramificacao:cache",
                "middleware:inline:depois",
                "middleware:classe:depois"
            },
            report.CachedRequest.Trace.ToArray());
    }
}
