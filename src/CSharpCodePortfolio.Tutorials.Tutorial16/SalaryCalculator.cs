namespace CSharpCodePortfolio.Tutorials.Tutorial16;

internal sealed class SalaryCalculator : ISalaryCalculator
{
    public decimal Calculate(SalaryInput input) =>
        input.HoursWorked * input.HourlyRate;
}
