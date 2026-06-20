using System.Reflection;
using EFCore10.Shared;
using Microsoft.Extensions.Configuration;

namespace EFCore10.Shared.Tests;

[TestClass]
public sealed class TutorialConfigurationTests
{
    [TestMethod]
    public void LoadForAssemblyReadsJsonFromAssemblyNamedOutputDirectory()
    {
        var assembly = typeof(TutorialConfigurationTests).Assembly;
        var assemblyName = assembly.GetName().Name
            ?? throw new InvalidOperationException("The test assembly name was not available.");
        var configurationDirectory = Path.Combine(AppContext.BaseDirectory, assemblyName);
        var configurationPath = Path.Combine(configurationDirectory, "custom-settings.json");

        Directory.CreateDirectory(configurationDirectory);
        File.WriteAllText(configurationPath, """
            {
              "ConnectionStrings": {
                "Default": "Data Source=test.db"
              }
            }
            """);

        var result = TutorialConfiguration.LoadForAssembly(assembly, "custom-settings.json");

        Assert.AreEqual(configurationDirectory, result.DirectoryPath);
        Assert.AreEqual("Data Source=test.db", result.Configuration.GetConnectionString("Default"));
    }
}
