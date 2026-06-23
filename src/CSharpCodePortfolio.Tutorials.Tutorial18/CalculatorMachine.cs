namespace CSharpCodePortfolio.Tutorials.Tutorial18;

internal sealed class CalculatorMachine(ICalculator calculator)
{
    public double Sum(double first, double second) =>
        calculator.Calculate(Add, first, second);

    public double Subtract(double first, double second) =>
        calculator.Calculate(SubtractNumbers, first, second);

    private static double Add(double first, double second) =>
        first + second;

    private static double SubtractNumbers(double first, double second) =>
        first - second;
}
