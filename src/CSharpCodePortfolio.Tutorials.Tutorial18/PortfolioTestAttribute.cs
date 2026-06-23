namespace CSharpCodePortfolio.Tutorials.Tutorial18;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class PortfolioTestAttribute(double first, double second, string scenario) : Attribute
{
    public double First { get; } = first;

    public double Second { get; } = second;

    public string Scenario { get; } = scenario;
}
