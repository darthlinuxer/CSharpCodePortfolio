using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace CSharpCodePortfolio.Tutorials.Tutorial29;

[Tutorial("29", "logging", "Boas Práticas de Logging")]
public sealed class Tutorial29 : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("29", "Boas Práticas de Logging");

        var configurationDirectory = Path.Combine(
            AppContext.BaseDirectory,
            typeof(Tutorial29).Assembly.GetName().Name!);

        TutorialConsole.WriteContext(
                   ("Slug", "logging"),
                   ("Pasta", Path.GetRelativePath(AppContext.BaseDirectory, configurationDirectory)),
                   ("Arquivo", "appsettings.json"));

        TutorialConsole.WriteQuestion(
         "Como evitar alocação de params object[] em logs Debug que não serão escritos?");

        TutorialConsole.WriteHypothesis(
            "ILogger.LogDebug com três int cria object[] e boxing antes de descartar Debug; LoggerAdapter usa overloads concretos e retorna antes dessa alocação.");

        TutorialConsole.WritePreparation(
            "O projeto copia appsettings.json para a pasta do tutorial na saída.");


        var configuration = new ConfigurationBuilder()
            .SetBasePath(configurationDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        using var serviceProvider = new ServiceCollection()
            .AddLogging(logging => logging
                .AddConfiguration(configuration.GetSection("Logging"))
                .AddSimpleConsole(options =>
                {
                    options.ColorBehavior = LoggerColorBehavior.Enabled;
                    options.SingleLine = true;
                }))
            .AddSingleton<LoggerAdapter<Tutorial29>>()
            .BuildServiceProvider(validateScopes: true);

        var logger = serviceProvider.GetRequiredService<ILogger<Tutorial29>>();
        var adapter = serviceProvider.GetRequiredService<LoggerAdapter<Tutorial29>>();
        var logAnalysis = LoggingMemoryComparison.Run(logger, adapter);

        TutorialConsole.WriteEvidence(
            "Configuração injetada",
            ("Logging:LogLevel:Default", configuration["Logging:LogLevel:Default"] ?? "(ausente)"),
            ("Logging:LogLevel:Microsoft.AspNetCore", configuration["Logging:LogLevel:Microsoft.AspNetCore"] ?? "(ausente)"),
            ("Serviço", nameof(LoggingExample<Tutorial29>)));

        TutorialConsole.WriteEvidence(
            "Cenário medido",
            ("Nível mínimo", configuration["Logging:LogLevel:Default"] ?? "(ausente)"),
            ("Chamadas Debug", LoggingMemoryComparison.DefaultIterations.ToString()),
            ("Argumentos", "3 int gerados por Random.Shared.Next()")
        );

        TutorialConsole.WriteEvidence(
            "ILogger padrão com Debug desabilitado",
            ("Bytes alocados", logAnalysis.result1.allocatedBytes.ToString())
        );

        TutorialConsole.WriteEvidence(
            "LoggerAdapter com overload sem params",
            ("Bytes alocados", logAnalysis.result2.allocatedBytes.ToString())
        );

        TutorialConsole.WriteEvidence(
            "Diferença medida",
            ("Bytes", logAnalysis.savedBytes.ToString())
        );

        TutorialConsole.WriteObservation(
            "Trace e Debug não aparecem; Information, Warning, Error e Critical aparecem abaixo.");

        new LoggingExample<Tutorial29>(adapter).WriteLogs();

        TutorialConsole.WriteConclusion(
            "ILogger padrão aloca por causa de params object[] e boxing. LoggerAdapter evita essa alocação porque o overload concreto checa IsEnabled antes do fallback params.");

        return Task.CompletedTask;
    }
}
