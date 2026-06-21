using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial04.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Tutorials.Tutorial04;

[Tutorial("04", "pooled-dynamic-state", "Estado dinâmico em DbContext pooled")]
public sealed class PooledDynamicStateTutorial : ITutorial
{
    private const string ConnectionStringName = "TutorialDatabase";
    internal const int DemoPoolSize = 1;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(PooledDynamicStateTutorial).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        TutorialConsole.WriteHeader("04", "Estado dinâmico em DbContext pooled");
        TutorialConsole.WriteContext(
            ("Provider", "SQLite"),
            ("Banco", SqliteConnectionStrings.GetDisplayDataSource(connectionString, AppContext.BaseDirectory)),
            ("Pool de DbContext", DemoPoolSize.ToString()));

        var services = new ServiceCollection();
        services.AddSqlitePooledDbContextFactory<BloggingContext>(connectionString, DemoPoolSize);
        services.AddSqlitePooledDbContextFactory<BadOnConfiguringTenantContext>(connectionString, DemoPoolSize);
        services.AddScoped<TenantAwareBloggingContextFactory>();
        services.AddScoped<PooledDynamicStateDemo>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var demo = scope.ServiceProvider.GetRequiredService<PooledDynamicStateDemo>();
        await demo.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
