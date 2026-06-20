using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial05.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Tutorials.Tutorial05;

[Tutorial("05", "connection-pooling", "DbContext Pooling vs Connection Pooling")]
public sealed class ConnectionPoolingTutorial : ITutorial
{
    private const string ConnectionStringName = "TutorialDatabase";
    private const int DemoPoolSize = 1;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(ConnectionPoolingTutorial).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        Console.WriteLine("05 - DbContext Pooling vs Connection Pooling");
        Console.WriteLine($"SQLite connection string: {connectionString}");
        Console.WriteLine($"DbContext pool size: {DemoPoolSize}");

        var services = new ServiceCollection();
        services.AddSqlitePooledDbContextFactory<PoolingContext>(connectionString, DemoPoolSize);
        services.AddScoped<ConnectionPoolingDemo>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var demo = scope.ServiceProvider.GetRequiredService<ConnectionPoolingDemo>();
        await demo.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
