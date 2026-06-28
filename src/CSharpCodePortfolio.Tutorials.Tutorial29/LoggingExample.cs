using Microsoft.Extensions.Logging;

namespace CSharpCodePortfolio.Tutorials.Tutorial29;

internal sealed class LoggingExample<T>(ILogger<T> logger)
{
    public void WriteLogs()
    {
        logger.LogTrace("Trace habilitado pelo appsettings.Development.json.");
        logger.LogDebug("Debug habilitado pela configuração base.");
        logger.LogInformation("Information registra fluxo esperado.");
        logger.LogWarning("Warning destaca atenção sem interromper execução.");
        logger.LogError("Error registra falha recuperável de demonstração.");
        logger.LogCritical("Critical registra falha grave de demonstração.");
    }
}
