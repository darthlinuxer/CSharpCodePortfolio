using Moq;

namespace CSharpCodePortfolio.Tutorials.Tutorial18;

internal static class CalculatorMachineTests
{
    [PortfolioTest(2, 3, "Soma de dois números")]
    private static ReflectedTestCase SumUsesMock(double first, double second)
    {
        return RunMockedCalculation(
            nameof(SumUsesMock),
            "Soma de dois números",
            first,
            second,
            machine => machine.Sum(first, second),
            expected: 5);
    }

    [PortfolioTest(8, 5, "Subtração de dois números")]
    private static ReflectedTestCase SubtractUsesMock(double first, double second)
    {
        return RunMockedCalculation(
            nameof(SubtractUsesMock),
            "Subtração de dois números",
            first,
            second,
            machine => machine.Subtract(first, second),
            expected: 3);
    }

    private static ReflectedTestCase RunMockedCalculation(
        string testName,
        string scenario,
        double first,
        double second,
        Func<CalculatorMachine, double> action,
        double expected)
    {
        var calculator = new Mock<ICalculator>(MockBehavior.Strict);
        calculator
            .Setup(calc => calc.Calculate(It.IsAny<Operation>(), first, second))
            .Returns((Operation operation, double a, double b) => operation(a, b));

        var result = action(new CalculatorMachine(calculator.Object));

        if (result != expected)
        {
            throw new InvalidOperationException($"Resultado esperado: {expected}; resultado obtido: {result}.");
        }

        calculator.Verify(
            calc => calc.Calculate(It.IsAny<Operation>(), first, second),
            Times.Once);
        calculator.VerifyNoOtherCalls();

        return new ReflectedTestCase(
            testName,
            scenario,
            result,
            $"ICalculator.Calculate({first:0.##}, {second:0.##}) uma vez");
    }
}
