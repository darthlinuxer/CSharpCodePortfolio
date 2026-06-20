using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial02.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Tutorials.Tutorial02;

[Tutorial("02", "Dependency Injection", "Adding DI and appsettings.json")]
public sealed class SecondTutorial : ITutorial
{
    private const string ConnectionStringName = "BloggingDatabase";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Tutorial 02 - EFCore Configuration");

        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(SecondTutorial).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        var services = new ServiceCollection();
        services.AddSqliteDbContext<BloggingContext>(connectionString);
        services.AddScoped<CRUD>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var crud = scope.ServiceProvider.GetRequiredService<CRUD>();
        await crud.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
