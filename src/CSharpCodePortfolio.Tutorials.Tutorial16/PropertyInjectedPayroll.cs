namespace CSharpCodePortfolio.Tutorials.Tutorial16;

internal sealed class PropertyInjectedPayroll
{
    public required ISalaryCalculator SalaryCalculator { private get; init; }

    public string CalculatorName => SalaryCalculator.GetType().Name;

    public decimal Calculate(SalaryInput input) =>
        SalaryCalculator.Calculate(input);
}
