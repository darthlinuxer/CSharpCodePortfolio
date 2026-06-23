using CSharpCodePortfolio.Tutorials.Tutorial21;

namespace CSharpCodePortfolio.Tutorials.Tutorial21.Tests;

[TestClass]
public sealed class MassTransitInMemoryScenarioTests
{
    [TestMethod]
    public async Task RunAsync_PublishesAndConsumesMessage()
    {
        var report = await new MassTransitInMemoryScenario().RunAsync(CancellationToken.None);

        Assert.AreEqual("portfolio-messages", report.QueueName);
        Assert.AreEqual(1, report.ConsumedMessages);
        Assert.AreEqual(report.Published.MessageId, report.Consumed.MessageId);
        Assert.AreEqual(report.Published.Text, report.Consumed.Text);
    }
}
