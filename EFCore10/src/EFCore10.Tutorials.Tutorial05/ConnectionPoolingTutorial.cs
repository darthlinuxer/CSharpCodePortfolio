using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial05.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Tutorials.Tutorial05;

[Tutorial("05", "connection-pooling", "DbContext pooling vs connection pooling")]
public sealed class ConnectionPoolingTutorial : ITutorial
{
    private const string ConnectionStringName = "TutorialDatabase";
    internal const int DemoPoolSize = 1;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(ConnectionPoolingTutorial).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        TutorialConsole.WriteHeader("05", "DbContext pooling vs connection pooling");
        TutorialConsole.WriteContext(
            ("Provider", "SQLite"),
            ("Banco", SqliteConnectionStrings.GetDisplayDataSource(connectionString, AppContext.BaseDirectory)),
            ("Pool de DbContext", DemoPoolSize.ToString()));

        var services = new ServiceCollection();
        services.AddSqlitePooledDbContextFactory<PoolingContext>(connectionString, DemoPoolSize);
        services.AddScoped<ConnectionPoolingDemo>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var demo = scope.ServiceProvider.GetRequiredService<ConnectionPoolingDemo>();
        await demo.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
