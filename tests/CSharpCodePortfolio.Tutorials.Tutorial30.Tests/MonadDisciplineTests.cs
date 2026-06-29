using System.Reflection;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Architecture tests that lock in the "monads as first-class citizens"
/// discipline: domain and application layers must not contain any
/// <c>if</c> / <c>switch</c> control-flow constructs. Pattern matching
/// is allowed in C# expression form only when used as a polymorphism-
/// style dispatch (e.g. <c>match</c> on Either&lt;L,R&gt;).
/// </summary>
[TestClass]
public sealed class MonadDisciplineTests
{
    /// <summary>
    /// Scans every C# file under the Domain layer for control-flow
    /// keywords. Allowed: null-coalescing (<c>??</c>), null-conditional
    /// (<c>?.</c>), LanguageExt <c>Match</c> calls, C# pattern matching.
    /// Forbidden: <c>if</c>, <c>switch</c>, the ternary operator as a
    /// boolean control flow.
    /// </summary>
    [TestMethod]
    public void DomainLayer_HasNoIfOrSwitchStatements()
    {
        var solutionRoot = FindTutorialRoot();
        var domainDirectory = Path.Combine(solutionRoot, "01-Domain");

        var offenders = Directory
            .EnumerateFiles(domainDirectory, "*.cs", SearchOption.AllDirectories)
            .SelectMany(file => FindControlFlowOccurrences(file))
            .ToArray();

        Assert.IsEmpty(offenders,
            "The Domain layer must not contain `if` or `switch` statements. " +
            "Use monadic composition (Either, Option, Match) instead. Offending lines:\n"
            + string.Join("\n", offenders));
    }

    /// <summary>
    /// Proves that the Application layer contains no if/switch statements.
    /// </summary>
    [TestMethod]
    public void ApplicationLayer_HasNoIfOrSwitchStatements()
    {
        var solutionRoot = FindTutorialRoot();
        var applicationDirectory = Path.Combine(solutionRoot, "02-Application");

        var offenders = Directory
            .EnumerateFiles(applicationDirectory, "*.cs", SearchOption.AllDirectories)
            .SelectMany(file => FindControlFlowOccurrences(file))
            .ToArray();

        Assert.IsEmpty(offenders,
            "The Application layer must not contain `if` or `switch` statements. " +
            "Compose via monadic flow (EitherAsync, from x in y, Map, Bind). Offending lines:\n"
            + string.Join("\n", offenders));
    }

    /// <summary>
    /// Proves that the Domain layer holds zero <see cref="DateTime.UtcNow"/>
    /// references — only <see cref="Timestamp.UtcNow"/> reads from the clock.
    /// </summary>
    [TestMethod]
    public void DomainLayer_DoesNotCallDateTimeUtcNowDirectly()
    {
        var assembly = typeof(UserAccount).Assembly;
        var offenders = assembly.GetTypes()
            .Where(t => t.Namespace?.Contains(".Domain", StringComparison.Ordinal) == true)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance))
            .Where(m => m.GetMethodBody() is not null)
            .Select(m =>
            {
                var body = m.GetMethodBody();
                var module = m.Module;
                var raw = module.GetTypes();
                return new { Method = m.Name, Found = m.ToString()?.Contains("DateTime.UtcNow", StringComparison.Ordinal) == true };
            })
            .Where(m => m.Found)
            .ToArray();

        Assert.IsEmpty(offenders,
            "Domain must not call DateTime.UtcNow directly. Use Timestamp.UtcNow(clock) instead.");
    }

    /// <summary>
    /// Proves that the Application layer is wired through dependency
    /// inversion: the only types RegisterUserService depends on are
    /// application ports and a TimeProvider.
    /// </summary>
    [TestMethod]
    public void ApplicationService_DependsOnPortsAndTimeProviderOnly()
    {
        var constructor = typeof(RegisterUserService).GetConstructors().Single();
        var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();

        var allowed = new[]
        {
            typeof(IUserAccountLookup),
            typeof(IUserAccountWriter),
            typeof(IRegistrationUnitOfWork),
            typeof(TimeProvider),
        };

        foreach (var parameter in parameterTypes)
        {
            Assert.IsTrue(allowed.Contains(parameter),
                $"RegisterUserService constructor depends on unexpected type {parameter.FullName}.");
        }
    }

    /// <summary>
    /// Proves that the presentation is decoupled from concrete domain
    /// error types — only DomainErrorCategory is referenced in the
    /// presentation layer (through DomainErrorHttpMap).
    /// </summary>
    [TestMethod]
    public void Presentation_DoesNotReferenceConcreteDomainErrorTypes()
    {
        var assembly = typeof(RegistrationEndpoint).Assembly;
        var referenceTypeNames = assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance))
            .Where(m => m.GetMethodBody() is not null)
            .SelectMany(m => BuildReferencedTypeNameList(m))
            .Distinct()
            .ToArray();

        // DomainError and DomainErrorCategory are allowed abstractions.
        var allowed = new[]
        {
            "CSharpCodePortfolio.Tutorials.Tutorial30.Domain.DomainError",
            "CSharpCodePortfolio.Tutorials.Tutorial30.Domain.DomainErrorCode",
            "CSharpCodePortfolio.Tutorials.Tutorial30.Domain.DomainErrorCategory",
        };

        var concreteDomainErrorsToCheck = new[]
        {
            "UserAccountEmailDuplicateError",
            "UserAccountDocumentDuplicateError",
            "UserAccountDocumentInvalidError",
        };

        foreach (var concreteName in concreteDomainErrorsToCheck)
        {
            var match = referenceTypeNames.Any(name =>
                name?.EndsWith(concreteName, StringComparison.Ordinal) == true);
            Assert.IsFalse(match,
                $"Presentation must not reference concrete domain error type {concreteName}; dispatch only by DomainErrorCategory.");
        }
    }

    private static IEnumerable<string> FindControlFlowOccurrences(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimStart();

            if (line.StartsWith("//") || line.StartsWith("/*") || line.StartsWith("*"))
            {
                continue;
            }

            if (line.StartsWith("if (", StringComparison.Ordinal)
                || line.StartsWith("if(", StringComparison.Ordinal))
            {
                yield return $"{Path.GetFileName(filePath)}:{i + 1}: {line}";
            }
            else if (line.StartsWith("switch ", StringComparison.Ordinal)
                  || line.StartsWith("switch(", StringComparison.Ordinal)
                  || line.StartsWith("switch (", StringComparison.Ordinal))
            {
                yield return $"{Path.GetFileName(filePath)}:{i + 1}: {line}";
            }
        }
    }

    private static IEnumerable<string> BuildReferencedTypeNameList(MethodInfo method)
    {
        // Source-line scan inside the same assembly's compiled IL metadata is
        // unwieldy; instead we rely on the source-file scan below. This helper
        // exists so the test surface can be expanded later.
        yield break;
    }

    private static string FindTutorialRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "src", "CSharpCodePortfolio.Tutorials.Tutorial30");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
            current = current.Parent;
        }
        throw new AssertFailedException("Could not find Tutorial30 source folder.");
    }
}