using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial02.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Tutorials.Tutorial02;

[Tutorial("02", "dependency-injection", "DI, appsettings e Fluent API")]
public sealed class SecondTutorial : ITutorial
{
    private const string ConnectionStringName = "BloggingDatabase";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(SecondTutorial).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        TutorialConsole.WriteHeader("02", "DI, appsettings e Fluent API");
        TutorialConsole.WriteContext(
            ("Provider", "SQLite"),
            ("Banco", SqliteConnectionStrings.GetDisplayDataSource(connectionString, AppContext.BaseDirectory)),
            ("Configuração", "appsettings.json"),
            ("Modelo", "IEntityTypeConfiguration<T>"));
        TutorialConsole.WriteQuestion(
            "Como mover a configuração do DbContext para appsettings, DI e Fluent API?");
        TutorialConsole.WriteHypothesis(
            "A connection string pode vir do appsettings.json.",
            "O container de DI pode criar o BloggingContext e injetá-lo no serviço CRUD.",
            "A Fluent API torna o modelo explícito sem depender apenas de convenções.");

        var services = new ServiceCollection();
        services.AddSqliteDbContext<BloggingContext>(connectionString);
        services.AddScoped<CRUD>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var crud = scope.ServiceProvider.GetRequiredService<CRUD>();
        await crud.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
