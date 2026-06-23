namespace CSharpCodePortfolio.Tutorials.Tutorial18;

internal sealed class Calculator : ICalculator
{
    public double Calculate(Operation operation, double first, double second) =>
        operation(first, second);
}
