using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace EFCore10.Shared;

/// <summary>
/// Resolves SQLite connection strings from application configuration.
/// </summary>
public static class SqliteConnectionStrings
{
    /// <summary>
    /// Gets a required connection string and makes relative SQLite data sources output-directory relative.
    /// </summary>
    public static string GetRequired(
        IConfiguration configuration,
        string name,
        string dataSourceBaseDirectory)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The connection string name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(dataSourceBaseDirectory))
            throw new ArgumentException("The SQLite data source base directory cannot be empty.", nameof(dataSourceBaseDirectory));

        var connectionString = configuration.GetConnectionString(name);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{name}' was not found.");

        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (ShouldResolveAgainstBaseDirectory(builder.DataSource))
        {
            Directory.CreateDirectory(dataSourceBaseDirectory);
            builder.DataSource = Path.Combine(dataSourceBaseDirectory, builder.DataSource);
        }

        return builder.ToString();
    }

    private static bool ShouldResolveAgainstBaseDirectory(string dataSource)
    {
        return !string.IsNullOrWhiteSpace(dataSource)
            && dataSource != ":memory:"
            && !Path.IsPathRooted(dataSource);
    }
}
