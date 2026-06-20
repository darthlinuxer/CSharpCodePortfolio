using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial04.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Tutorials.Tutorial04;

[Tutorial("04", "pooled-dynamic-state", "Pooled Contexts and Dynamic State")]
public sealed class PooledDynamicStateTutorial : ITutorial
{
    private const string ConnectionStringName = "TutorialDatabase";
    private const int DemoPoolSize = 1;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(PooledDynamicStateTutorial).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        Console.WriteLine("04 - Pooled Contexts and Dynamic State");
        Console.WriteLine($"SQLite connection string: {connectionString}");
        Console.WriteLine($"DbContext pool size: {DemoPoolSize}");

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
