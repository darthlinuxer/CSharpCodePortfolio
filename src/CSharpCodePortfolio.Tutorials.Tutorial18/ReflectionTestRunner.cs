using System.Reflection;

namespace CSharpCodePortfolio.Tutorials.Tutorial18;

internal static class ReflectionTestRunner
{
    public static TestRunReport Run()
    {
        var testType = typeof(CalculatorMachineTests);
        var cases = testType
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(static method => method.IsDefined(typeof(PortfolioTestAttribute), inherit: false))
            .OrderBy(static method => method.Name, StringComparer.Ordinal)
            .Select(InvokeTest)
            .ToArray();

        return new TestRunReport(testType.Name, cases.Length, cases.Length, cases);
    }

    private static ReflectedTestCase InvokeTest(MethodInfo method)
    {
        var test = method.GetCustomAttribute<PortfolioTestAttribute>()
            ?? throw new InvalidOperationException($"Método sem atributo de teste: {method.Name}.");

        var result = method.Invoke(null, [test.First, test.Second]);
        return result as ReflectedTestCase
            ?? throw new InvalidOperationException($"Teste sem relatório: {method.Name}.");
    }
}
