using CSharpCodePortfolio.Tutorials.Tutorial24;

namespace CSharpCodePortfolio.Tutorials.Tutorial24.Tests;

[TestClass]
public sealed class RavenConnectionScenarioTests
{
    [TestMethod]
    public void Run_CreatesRavenConnectionPlan()
    {
        var report = RavenConnectionScenario.Run();

        Assert.AreEqual("UserDatabase", report.Database);
        CollectionAssert.AreEqual(
            new[] { "http://localhost:9900", "http://localhost:8080" },
            report.HostUrls.ToArray());
        CollectionAssert.AreEqual(
            new[] { "http://raven_A:8080", "http://raven_B:8080" },
            report.ContainerUrls.ToArray());
        Assert.AreEqual(10, report.MaxRequestsPerSession);
        Assert.IsTrue(report.UseOptimisticConcurrency);
        CollectionAssert.AreEqual(
            new[]
            {
                "OpenSession()",
                "Store(users/1-A)",
                "SaveChanges()",
                "Query<RavenUser>().Skip(0).Take(10)",
                "Load<RavenUser>(users/1-A)",
                "Delete(users/1-A)",
                "SaveChanges()"
            },
            report.SessionSteps.ToArray());
    }
}
