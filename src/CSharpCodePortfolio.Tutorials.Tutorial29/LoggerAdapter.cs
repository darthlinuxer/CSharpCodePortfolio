using Microsoft.Extensions.Logging;

namespace CSharpCodePortfolio.Tutorials.Tutorial29;

public sealed class LoggerAdapter<T>(ILogger<T> logger) : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => logger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => logger.IsEnabled(logLevel);

    public void LogTrace(string? message) => LogIfEnabled(LogLevel.Trace, message);
    public void LogTrace<T1>(string? message, T1 arg1) => LogIfEnabled(LogLevel.Trace, message, arg1);
    public void LogTrace<T1, T2>(string? message, T1 arg1, T2 arg2) => LogIfEnabled(LogLevel.Trace, message, arg1, arg2);
    public void LogTrace<T1, T2, T3>(string? message, T1 arg1, T2 arg2, T3 arg3) => LogIfEnabled(LogLevel.Trace, message, arg1, arg2, arg3);
    public void LogTrace(string? message, params object?[] args) => LogIfEnabledWithArgs(LogLevel.Trace, message, args);

    public void LogDebug(string? message) => LogIfEnabled(LogLevel.Debug, message);
    public void LogDebug<T1>(string? message, T1 arg1) => LogIfEnabled(LogLevel.Debug, message, arg1);
    public void LogDebug<T1, T2>(string? message, T1 arg1, T2 arg2) => LogIfEnabled(LogLevel.Debug, message, arg1, arg2);
    public void LogDebug<T1, T2, T3>(string? message, T1 arg1, T2 arg2, T3 arg3) => LogIfEnabled(LogLevel.Debug, message, arg1, arg2, arg3);
    public void LogDebug(string? message, params object?[] args) => LogIfEnabledWithArgs(LogLevel.Debug, message, args);

    public void LogInformation(string? message) => LogIfEnabled(LogLevel.Information, message);
    public void LogInformation<T1>(string? message, T1 arg1) => LogIfEnabled(LogLevel.Information, message, arg1);
    public void LogInformation<T1, T2>(string? message, T1 arg1, T2 arg2) => LogIfEnabled(LogLevel.Information, message, arg1, arg2);
    public void LogInformation<T1, T2, T3>(string? message, T1 arg1, T2 arg2, T3 arg3) => LogIfEnabled(LogLevel.Information, message, arg1, arg2, arg3);
    public void LogInformation(string? message, params object?[] args) => LogIfEnabledWithArgs(LogLevel.Information, message, args);

    public void LogWarning(string? message) => LogIfEnabled(LogLevel.Warning, message);
    public void LogWarning<T1>(string? message, T1 arg1) => LogIfEnabled(LogLevel.Warning, message, arg1);
    public void LogWarning<T1, T2>(string? message, T1 arg1, T2 arg2) => LogIfEnabled(LogLevel.Warning, message, arg1, arg2);
    public void LogWarning<T1, T2, T3>(string? message, T1 arg1, T2 arg2, T3 arg3) => LogIfEnabled(LogLevel.Warning, message, arg1, arg2, arg3);
    public void LogWarning(string? message, params object?[] args) => LogIfEnabledWithArgs(LogLevel.Warning, message, args);

    public void LogError(string? message) => LogIfEnabled(LogLevel.Error, message);
    public void LogError<T1>(string? message, T1 arg1) => LogIfEnabled(LogLevel.Error, message, arg1);
    public void LogError<T1, T2>(string? message, T1 arg1, T2 arg2) => LogIfEnabled(LogLevel.Error, message, arg1, arg2);
    public void LogError<T1, T2, T3>(string? message, T1 arg1, T2 arg2, T3 arg3) => LogIfEnabled(LogLevel.Error, message, arg1, arg2, arg3);
    public void LogError(string? message, params object?[] args) => LogIfEnabledWithArgs(LogLevel.Error, message, args);

    public void LogCritical(string? message) => LogIfEnabled(LogLevel.Critical, message);
    public void LogCritical<T1>(string? message, T1 arg1) => LogIfEnabled(LogLevel.Critical, message, arg1);
    public void LogCritical<T1, T2>(string? message, T1 arg1, T2 arg2) => LogIfEnabled(LogLevel.Critical, message, arg1, arg2);
    public void LogCritical<T1, T2, T3>(string? message, T1 arg1, T2 arg2, T3 arg3) => LogIfEnabled(LogLevel.Critical, message, arg1, arg2, arg3);
    public void LogCritical(string? message, params object?[] args) => LogIfEnabledWithArgs(LogLevel.Critical, message, args);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        logger.Log(logLevel, eventId, state, exception, formatter);
    }

    private void LogIfEnabled(LogLevel logLevel, string? message)
    {
        if (!IsEnabled(logLevel))
            return;

        LoggerExtensions.Log(this, logLevel, message);
    }

    private void LogIfEnabled<T1>(LogLevel logLevel, string? message, T1 arg1)
    {
        if (!IsEnabled(logLevel))
            return;

        LoggerExtensions.Log(this, logLevel, message, arg1);
    }

    private void LogIfEnabled<T1, T2>(LogLevel logLevel, string? message, T1 arg1, T2 arg2)
    {
        if (!IsEnabled(logLevel))
            return;

        LoggerExtensions.Log(this, logLevel, message, arg1, arg2);
    }

    private void LogIfEnabled<T1, T2, T3>(LogLevel logLevel, string? message, T1 arg1, T2 arg2, T3 arg3)
    {
        if (!IsEnabled(logLevel))
            return;

        LoggerExtensions.Log(this, logLevel, message, arg1, arg2, arg3);
    }

    private void LogIfEnabledWithArgs(LogLevel logLevel, string? message, object?[] args)
    {
        if (!IsEnabled(logLevel))
            return;

        LoggerExtensions.Log(this, logLevel, message, args);
    }
}
