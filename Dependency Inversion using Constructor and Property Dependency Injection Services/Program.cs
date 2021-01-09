//Title: Dependency Inversion using Dependency Injection Services for Constructor and Property

using System;
using Microsoft.Extensions.DependencyInjection;

public interface ISalaryCalculator { float CalculateSalary(int hoursWorked, float hourlyRate); }
public class SalaryCalculator : ISalaryCalculator, IDisposable
{
	public float CalculateSalary(int hoursWorked, float hourlyRate) => hoursWorked * hourlyRate;
	public void Dispose() => Console.WriteLine("SalaryCalculator class is being disposed");
}

public class EmployeeDetails : IDisposable //Normal Constructor Injection
{
	private readonly ISalaryCalculator _salaryCalculator;
	public int HoursWorked { get; set; }
	public int HourlyRate { get; set; }
	public EmployeeDetails(ISalaryCalculator salaryCalculator) => _salaryCalculator = salaryCalculator; //Constructor Injection
	public float GetSalary() => _salaryCalculator.CalculateSalary(HoursWorked, HourlyRate);
	public void Dispose() => Console.WriteLine("EmployeeDetails class is being disposed");
}

public class EmployeeDetailsPropertyInjection : IDisposable
{
	public ISalaryCalculator SalaryCalculator { get; } //Property Injection
	public int HoursWorked { get; set; }
	public int HourlyRate { get; set; }
	public EmployeeDetailsPropertyInjection(ISalaryCalculator salaryCalc) => SalaryCalculator = salaryCalc;
	public float GetSalary() => SalaryCalculator.CalculateSalary(HoursWorked, HourlyRate);
	public void Dispose() => Console.WriteLine("EmployeeDetailsPropertyInjection class is being disposed");
}

public class Program
{
	public static void Main()
	{

		var serviceProvider = new ServiceCollection()
			.AddTransient<ISalaryCalculator, SalaryCalculator>()
			.AddSingleton<EmployeeDetails>() //Constructor Injection
			.AddScoped<EmployeeDetailsPropertyInjection>(implementationFactory: provider => {
				var myCalculatorService = provider.GetRequiredService<ISalaryCalculator>();
				return new EmployeeDetailsPropertyInjection(myCalculatorService);
			})
			.BuildServiceProvider();

		using (EmployeeDetails employeeDetails = serviceProvider.GetService<EmployeeDetails>())
		{
			employeeDetails.HourlyRate = 50;
			employeeDetails.HoursWorked = 150;
			Console.WriteLine($"The Total Pay is {employeeDetails.GetSalary()}");
		}

		using (EmployeeDetailsPropertyInjection employeeDetails2 = serviceProvider.GetService<EmployeeDetailsPropertyInjection>())
		{
			employeeDetails2.HourlyRate = 100;
			employeeDetails2.HoursWorked = 150;
			Console.WriteLine($"The Total Pay is {employeeDetails2.GetSalary()}");
		}

	}
}
