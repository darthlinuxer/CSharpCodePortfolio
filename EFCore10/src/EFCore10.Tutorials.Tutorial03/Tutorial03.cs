using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial03.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Tutorials.Tutorial03;

[Tutorial("03", "pooled-dbcontext-factory", "Factory pooled de DbContext")]
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

        TutorialConsole.WriteHeader("03", "Factory pooled de DbContext");
        TutorialConsole.WriteContext(
            ("Provider", "SQLite"),
            ("Banco", SqliteConnectionStrings.GetDisplayDataSource(connectionString, AppContext.BaseDirectory)),
            ("Pool de DbContext", DemoPoolSize.ToString()));

        var services = new ServiceCollection();
        services.AddSqlitePooledDbContextFactory<BloggingContext>(connectionString, DemoPoolSize);
        services.AddScoped<PooledFactoryDemo>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var demo = scope.ServiceProvider.GetRequiredService<PooledFactoryDemo>();
        await demo.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
