using CSharpCodePortfolio.Tutorials.Tutorial28;

namespace CSharpCodePortfolio.Tutorials.Tutorial28.Tests;

[TestClass]
public sealed class ParallelismScenarioTests
{
    [TestMethod]
    public async Task RunAsync_ProducesParallelismReport()
    {
        var report = await ParallelismScenario.RunAsync();

        Assert.HasCount(3, report.IndependentActions);
        Assert.AreEqual(1, report.Sequential.MaxConcurrency);
        Assert.AreEqual(4, report.FanOut.MaxConcurrency);
        Assert.AreEqual(2, report.Limited.MaxConcurrency);
        Assert.AreEqual(17, report.Sequential.TotalScore);
        Assert.AreEqual(report.Sequential.TotalScore, report.FanOut.TotalScore);
        Assert.AreEqual(report.Sequential.TotalScore, report.Limited.TotalScore);
        CollectionAssert.AreEqual(new[] { 0, 2, 4, 6, 8 }, report.Partition.EvenNumbers.ToArray());
        CollectionAssert.AreEqual(new[] { 1, 3, 5, 7, 9 }, report.Partition.OddNumbers.ToArray());
        CollectionAssert.AreEqual(new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 }, report.Primes.ToArray());
        Assert.IsTrue(report.CancellationObserved);
        Assert.HasCount(4, report.Checklist);
    }

    [TestMethod]
    public async Task RunWithForEachAsync_RespectsMaxDegreeOfParallelism()
    {
        var run = await ParallelismScenario.RunWithForEachAsync(
            ParallelismScenario.CreateWorkItems(),
            maxDegreeOfParallelism: 2);

        Assert.AreEqual("Parallel.ForEachAsync", run.Strategy);
        Assert.AreEqual(2, run.MaxConcurrency);
        Assert.AreEqual(17, run.TotalScore);
    }

    [TestMethod]
    public async Task RunWithWhenAllAsync_StartsAllWorkItems()
    {
        var run = await ParallelismScenario.RunWithWhenAllAsync(ParallelismScenario.CreateWorkItems());

        Assert.AreEqual("Task.WhenAll", run.Strategy);
        Assert.AreEqual(4, run.MaxConcurrency);
        Assert.AreEqual(17, run.TotalScore);
    }

    [TestMethod]
    public void FindPrimesUntil_RejectsInvalidDegree()
    {
        var counter = new CpuPrimeCounter();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => counter.FindPrimesUntil(30, maxDegreeOfParallelism: 0));
    }
}
