namespace CSharpCodePortfolio.Tutorials.Tutorial16;

internal sealed class ConstructorInjectedPayroll(ISalaryCalculator salaryCalculator)
{
    public string CalculatorName => salaryCalculator.GetType().Name;

    public decimal Calculate(SalaryInput input) =>
        salaryCalculator.Calculate(input);
}
