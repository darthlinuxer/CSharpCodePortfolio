using System.Net;
using CSharpCodePortfolio.Tutorials.Tutorial23;

namespace CSharpCodePortfolio.Tutorials.Tutorial23.Tests;

[TestClass]
public sealed class RazorPagesScenarioTests
{
    [TestMethod]
    public async Task RunAsync_RendersPageModelWithLayout()
    {
        var report = await new RazorPagesScenario().RunAsync(CancellationToken.None);

        Assert.StartsWith("http://127.0.0.1:", report.BaseAddress);
        Assert.AreEqual(HttpStatusCode.OK, report.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, report.PrivacyStatusCode);
        Assert.AreEqual("Página inicial - Tutorial Razor Pages", report.Title);
        Assert.IsTrue(report.HasLayout);
        Assert.IsTrue(report.HasNavigation);
        Assert.IsTrue(report.HasPageModelContent);
        Assert.IsTrue(report.HasRazorPageMarker);
    }
}
