using CSharpCodePortfolio.Tutorials.Tutorial29;
using Microsoft.Extensions.Logging;

namespace CSharpCodePortfolio.Tutorials.Tutorial29.Tests;

[TestClass]
public sealed class LoggerAdapterTests
{
    [TestMethod]
    public void LoggerAdapter_CanUseStandardExtensionSignatures_WhenEnabled()
    {
        var logger = new FakeLogger<LoggerAdapterTests>(enabledLevel: LogLevel.Trace);
        var adapter = new LoggerAdapter<LoggerAdapterTests>(logger);
        var exception = new InvalidOperationException("boom");

        adapter.LogInformation("Olá {Name}", "Ada");
        adapter.LogWarning(new EventId(7), "Aviso {Code}", 42);
        adapter.LogError(exception, "Erro {Name}", "Ada");
        adapter.LogCritical(new EventId(9), exception, "Crítico {Name}", "Ada");
        adapter.LogDebug("Debug {Name}", "Ada");

        Assert.HasCount(5, logger.Entries);
        Assert.AreEqual(LogLevel.Information, logger.Entries[0].Level);
        Assert.AreEqual("Olá Ada", logger.Entries[0].Message);
        Assert.AreEqual(LogLevel.Warning, logger.Entries[1].Level);
        Assert.AreEqual("Aviso 42", logger.Entries[1].Message);
        Assert.AreEqual(LogLevel.Error, logger.Entries[2].Level);
        Assert.AreEqual("Erro Ada", logger.Entries[2].Message);
        Assert.AreEqual(LogLevel.Critical, logger.Entries[3].Level);
        Assert.AreEqual("Crítico Ada", logger.Entries[3].Message);
        Assert.AreEqual(LogLevel.Debug, logger.Entries[4].Level);
        Assert.AreEqual("Debug Ada", logger.Entries[4].Message);
    }

    [TestMethod]
    public void LoggerAdapter_DeclaresLowAllocationInstanceMethods_ForConcreteCalls()
    {
        foreach (var methodName in LogMethodNames)
        {
            AssertLevelOverloadsDeclared(methodName);
        }
    }

    [TestMethod]
    public void LoggerAdapter_DoesNotForwardDisabledStandardLogExtension()
    {
        var logger = new FakeLogger<LoggerAdapterTests>(
            enabledLevel: LogLevel.Information,
            recordDisabledEntries: true);
        var adapter = new LoggerAdapter<LoggerAdapterTests>(logger);

        adapter.LogDebug("debug invisível");

        Assert.IsEmpty(logger.Entries);
    }

    [TestMethod]
    public void LoggerAdapter_DisabledZeroToThreeArgumentLevelLogs_DoNotAllocate()
    {
        var logger = new FakeLogger<LoggerAdapterTests>(enabledLevel: LogLevel.None);
        var adapter = new LoggerAdapter<LoggerAdapterTests>(logger);

        RunDisabledLowAllocationCalls(adapter);
        var before = GC.GetAllocatedBytesForCurrentThread();

        RunDisabledLowAllocationCalls(adapter);

        Assert.AreEqual(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.IsEmpty(logger.Entries);
    }

    [TestMethod]
    public void LoggerAdapter_LevelOverloads_LogExpectedLevels_WhenEnabled()
    {
        var logger = new FakeLogger<LoggerAdapterTests>(enabledLevel: LogLevel.Trace);
        var adapter = new LoggerAdapter<LoggerAdapterTests>(logger);

        adapter.LogTrace("Trace {Value}", 1);
        adapter.LogDebug("Debug {First} {Second}", 1, 2);
        adapter.LogInformation("Information {A} {B} {C}", 1, 2, 3);
        adapter.LogWarning("Warning");
        adapter.LogError("Error {Value}", 1);
        adapter.LogCritical("Critical {First} {Second}", 1, 2);

        Assert.HasCount(6, logger.Entries);
        Assert.AreEqual(new LogEntry(LogLevel.Trace, "Trace 1"), logger.Entries[0]);
        Assert.AreEqual(new LogEntry(LogLevel.Debug, "Debug 1 2"), logger.Entries[1]);
        Assert.AreEqual(new LogEntry(LogLevel.Information, "Information 1 2 3"), logger.Entries[2]);
        Assert.AreEqual(new LogEntry(LogLevel.Warning, "Warning"), logger.Entries[3]);
        Assert.AreEqual(new LogEntry(LogLevel.Error, "Error 1"), logger.Entries[4]);
        Assert.AreEqual(new LogEntry(LogLevel.Critical, "Critical 1 2"), logger.Entries[5]);
    }

    [TestMethod]
    public void LoggingMemoryComparison_DoesNotUsePayloadFactory_WhenDebugDisabled()
    {
        var logger1 = new FakeLogger<LoggingExample<Tutorial29>>(enabledLevel: LogLevel.Information);
        var innerLogger = new FakeLogger<LoggingExample<Tutorial29>>(enabledLevel: LogLevel.Information);
        var logger2 = new LoggerAdapter<LoggingExample<Tutorial29>>(innerLogger);

        var result = LoggingMemoryComparison.Run(logger1, logger2, iterations: 4);

        Assert.AreEqual(0, result.result1.factoryCalls);
        Assert.AreEqual(0, result.result2.factoryCalls);
        Assert.IsGreaterThan(0, result.result1.allocatedBytes);
        Assert.AreEqual(0, result.result2.allocatedBytes);
        Assert.IsEmpty(logger1.Entries);
        Assert.IsEmpty(innerLogger.Entries);
    }

    [TestMethod]
    public void LoggingMemoryComparison_DoesNotUsePayloadFactory_WhenDebugEnabled()
    {
        var logger1 = new FakeLogger<LoggingExample<Tutorial29>>(enabledLevel: LogLevel.Debug);
        var innerLogger = new FakeLogger<LoggingExample<Tutorial29>>(enabledLevel: LogLevel.Debug);
        var logger2 = new LoggerAdapter<LoggingExample<Tutorial29>>(innerLogger);

        var result = LoggingMemoryComparison.Run(logger1, logger2, iterations: 4);

        Assert.AreEqual(0, result.result1.factoryCalls);
        Assert.AreEqual(0, result.result2.factoryCalls);
        Assert.HasCount(4, logger1.Entries);
        Assert.HasCount(4, innerLogger.Entries);
    }

    private sealed class FakeLogger<T>(
        LogLevel enabledLevel,
        bool recordDisabledEntries = false) : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= enabledLevel;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!recordDisabledEntries && !IsEnabled(logLevel))
            {
                return;
            }

            Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message);

    private static readonly string[] LogMethodNames =
    [
        nameof(LoggerExtensions.LogTrace),
        nameof(LoggerExtensions.LogDebug),
        nameof(LoggerExtensions.LogInformation),
        nameof(LoggerExtensions.LogWarning),
        nameof(LoggerExtensions.LogError),
        nameof(LoggerExtensions.LogCritical)
    ];

    private static void AssertLevelOverloadsDeclared(string methodName)
    {
        var methods = typeof(LoggerAdapter<LoggerAdapterTests>)
            .GetMethods()
            .Where(method => method.Name == methodName)
            .ToArray();

        Assert.IsTrue(methods.Any(method => IsPlainMessageOverload(method, parameterCount: 1)), methodName);
        Assert.IsTrue(methods.Any(method => IsGenericMessageOverload(method, genericArgumentCount: 1)), methodName);
        Assert.IsTrue(methods.Any(method => IsGenericMessageOverload(method, genericArgumentCount: 2)), methodName);
        Assert.IsTrue(methods.Any(method => IsGenericMessageOverload(method, genericArgumentCount: 3)), methodName);
        Assert.IsTrue(methods.Any(IsParamsFallbackOverload), methodName);
    }

    private static bool IsPlainMessageOverload(System.Reflection.MethodInfo method, int parameterCount)
    {
        var parameters = method.GetParameters();

        return !method.IsGenericMethod
            && parameters.Length == parameterCount
            && parameters[0].ParameterType == typeof(string);
    }

    private static bool IsGenericMessageOverload(System.Reflection.MethodInfo method, int genericArgumentCount)
    {
        var parameters = method.GetParameters();

        return method.IsGenericMethodDefinition
            && method.GetGenericArguments().Length == genericArgumentCount
            && parameters.Length == genericArgumentCount + 1
            && parameters[0].ParameterType == typeof(string);
    }

    private static bool IsParamsFallbackOverload(System.Reflection.MethodInfo method)
    {
        var parameters = method.GetParameters();

        return !method.IsGenericMethod
            && parameters.Length == 2
            && parameters[0].ParameterType == typeof(string)
            && parameters[1].ParameterType == typeof(object[]);
    }

    private static void RunDisabledLowAllocationCalls(LoggerAdapter<LoggerAdapterTests> adapter)
    {
        adapter.LogTrace("trace");
        adapter.LogTrace("trace {A}", 1);
        adapter.LogTrace("trace {A} {B}", 1, 2);
        adapter.LogTrace("trace {A} {B} {C}", 1, 2, 3);
        adapter.LogDebug("debug");
        adapter.LogDebug("debug {A}", 1);
        adapter.LogDebug("debug {A} {B}", 1, 2);
        adapter.LogDebug("debug {A} {B} {C}", 1, 2, 3);
        adapter.LogInformation("information");
        adapter.LogInformation("information {A}", 1);
        adapter.LogInformation("information {A} {B}", 1, 2);
        adapter.LogInformation("information {A} {B} {C}", 1, 2, 3);
        adapter.LogWarning("warning");
        adapter.LogWarning("warning {A}", 1);
        adapter.LogWarning("warning {A} {B}", 1, 2);
        adapter.LogWarning("warning {A} {B} {C}", 1, 2, 3);
        adapter.LogError("error");
        adapter.LogError("error {A}", 1);
        adapter.LogError("error {A} {B}", 1, 2);
        adapter.LogError("error {A} {B} {C}", 1, 2, 3);
        adapter.LogCritical("critical");
        adapter.LogCritical("critical {A}", 1);
        adapter.LogCritical("critical {A} {B}", 1, 2);
        adapter.LogCritical("critical {A} {B} {C}", 1, 2, 3);
    }
}
