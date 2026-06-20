using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace EFCore10.Shared;

/// <summary>
/// Loads tutorial configuration files copied under the assembly-named output directory.
/// </summary>
public static class TutorialConfiguration
{
    /// <summary>
    /// Loads a JSON configuration file from <c>AppContext.BaseDirectory/{assembly name}</c>.
    /// </summary>
    public static TutorialConfigurationResult LoadForAssembly(Assembly assembly, string fileName = "appsettings.json")
    {
        ArgumentNullException.ThrowIfNull(assembly);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("The configuration file name cannot be empty.", nameof(fileName));

        var assemblyName = assembly.GetName().Name
            ?? throw new InvalidOperationException("The assembly name was not available.");
        var directoryPath = Path.Combine(AppContext.BaseDirectory, assemblyName);
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directoryPath)
            .AddJsonFile(fileName, optional: false, reloadOnChange: false)
            .Build();

        return new TutorialConfigurationResult(configuration, directoryPath);
    }
}
