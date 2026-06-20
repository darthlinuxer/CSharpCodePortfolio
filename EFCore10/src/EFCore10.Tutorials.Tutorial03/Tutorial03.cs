using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial03.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Tutorials.Tutorial03;

[Tutorial("03", "Factory", "Pooled DbContext with Factory")]
public sealed class Tutorial03 : ITutorial
{
    private const string ConnectionStringName = "TutorialDatabase";
    private const int DemoPoolSize = 1;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(Tutorial03).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        Console.WriteLine("Tutorial 03 - Pooled DbContext Factory");
        Console.WriteLine($"SQLite connection string: {connectionString}");
        Console.WriteLine($"DbContext pool size: {DemoPoolSize}");
        Console.WriteLine("Each operation creates and disposes a context through IDbContextFactory.");
        Console.WriteLine("With a pool size of 1, the same instance can be reused after disposal.");

        var services = new ServiceCollection();
        services.AddSqlitePooledDbContextFactory<BloggingContext>(connectionString, DemoPoolSize);
        services.AddScoped<PooledFactoryDemo>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var demo = scope.ServiceProvider.GetRequiredService<PooledFactoryDemo>();
        await demo.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
