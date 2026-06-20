using EFCore10.Shared;
using Microsoft.Extensions.Configuration;

namespace EFCore10.Shared.Tests;

[TestClass]
public sealed class SqliteConnectionStringsTests
{
    [TestMethod]
    public void GetRequiredThrowsWhenConnectionStringIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => SqliteConnectionStrings.GetRequired(configuration, "Missing", "unused"));

        Assert.Contains("Connection string 'Missing' was not found.", exception.Message);
    }

    [TestMethod]
    public void GetRequiredResolvesRelativeDataSourceAgainstBaseDirectory()
    {
        var baseDirectory = CreateUniqueDirectoryPath();
        var configuration = CreateConfiguration("Data Source=blogging.db");

        var connectionString = SqliteConnectionStrings.GetRequired(configuration, "Default", baseDirectory);

        Assert.IsTrue(Directory.Exists(baseDirectory));
        StringAssert.Contains(connectionString, Path.Combine(baseDirectory, "blogging.db"));
    }

    [TestMethod]
    public void GetRequiredPreservesInMemoryDataSource()
    {
        var baseDirectory = CreateUniqueDirectoryPath();
        var configuration = CreateConfiguration("Data Source=:memory:");

        var connectionString = SqliteConnectionStrings.GetRequired(configuration, "Default", baseDirectory);

        StringAssert.Contains(connectionString, ":memory:");
        Assert.IsFalse(Directory.Exists(baseDirectory));
    }

    [TestMethod]
    public void GetRequiredPreservesAbsoluteDataSource()
    {
        var baseDirectory = CreateUniqueDirectoryPath();
        var absoluteDataSource = Path.Combine(CreateUniqueDirectoryPath(), "absolute.db");
        var configuration = CreateConfiguration($"Data Source={absoluteDataSource}");

        var connectionString = SqliteConnectionStrings.GetRequired(configuration, "Default", baseDirectory);

        StringAssert.Contains(connectionString, absoluteDataSource);
        Assert.IsFalse(Directory.Exists(baseDirectory));
    }

    private static IConfiguration CreateConfiguration(string connectionString)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString
            })
            .Build();
    }

    private static string CreateUniqueDirectoryPath()
    {
        return Path.Combine(Path.GetTempPath(), "efcore10-shared-tests", Guid.NewGuid().ToString("N"));
    }
}
