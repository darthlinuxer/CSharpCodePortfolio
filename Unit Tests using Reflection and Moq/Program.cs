using static System.Console;
using Moq;
using NUnit.Framework;
using System.Linq;
using System;

public delegate double Operation(double a, double b);
public interface ICalculator { (Operation operation, double result) Calculate(Operation op, double a, double b); }
public class Calculator : ICalculator { public (Operation operation, double result) Calculate(Operation op, double a, double b) => (op, op(a, b)); }
public class CalculatorMachine
{
	public ICalculator Brain { get;  }
	public CalculatorMachine(ICalculator calc) => Brain = calc;
	public double Sum(double a, double b) => a + b;
	public double Subtract(double a, double b) => a - b;
	public double Multiply(double a, double b) => a * b;
	public double Divide(double a, double b) => a / b;
}

public class UnitTests
{

	public void Sum_2_Numbers(double a, double b)
	{
		// Arrange
		Moq.Mock<ICalculator> mockCalculator = new Moq.Mock<ICalculator>();
		CalculatorMachine maqCalc = new CalculatorMachine(mockCalculator.Object);
		mockCalculator.Setup(x => x.Calculate(It.Is<Operation>(op => op == maqCalc.Sum), It.IsAny<double>(), It.IsAny<double>())).Returns((maqCalc.Sum, a + b));
		// Act
		(Operation op, double result) = maqCalc.Brain.Calculate(maqCalc.Sum, a, b);
		// Assert            
		WriteLine($"Sum_2_Numbers: Checking if operation is Sum: {(op == maqCalc.Sum)}");
		Assert.IsTrue(op == maqCalc.Sum); //You don't actually need this, but I've let it because in case it fails it will create an Exception which shows a message
		WriteLine($"Sum_2_Numbers: Checking operation result: {a} + {b} = {result}");
		Assert.IsTrue(result == a + b);
	}

	public void Subtract_2_Numbers(double a, double b)
	{
		// Arrange
		Moq.Mock<ICalculator> mockCalculator = new Moq.Mock<ICalculator>();
		CalculatorMachine maqCalc = new CalculatorMachine(mockCalculator.Object);
		mockCalculator.Setup(x => x.Calculate(It.Is<Operation>(op => op == maqCalc.Subtract), It.IsAny<double>(), It.IsAny<double>())).Returns((maqCalc.Subtract, a - b));
		// Act
		(Operation op, double result) = maqCalc.Brain.Calculate(maqCalc.Subtract, a, b);
		// Assert            
		WriteLine($"Subtract_2_Numbers: Checking if operation is Subtract: {(op == maqCalc.Subtract)}");
		Assert.IsTrue(op == maqCalc.Subtract);
		WriteLine($"Subtract_2_Numbers: Checking operation result: {a} - {b} = {result}");
		Assert.IsTrue(result == a - b);
	}
}

public class Program
{
	public static void Main()
	{
		//DIRECT CALLS
		WriteLine("Direct Calls");
		var calculator = new CalculatorMachine(new Calculator());
		WriteLine($"Sum:" + calculator.Brain.Calculate(calculator.Sum, 2, 2).result);
		WriteLine($"Subtract:" + calculator.Brain.Calculate(calculator.Subtract, 2, 2).result);
		WriteLine($"Multiply:" + calculator.Brain.Calculate(calculator.Multiply, 2, 2).result);
		WriteLine($"Divide:" + calculator.Brain.Calculate(calculator.Divide, 2, 2).result);

		WriteLine(new String('-', 100));
		WriteLine("Starting the Tests!");
		var unitTests = new UnitTests();
		foreach (var t in typeof(UnitTests).GetMethods().Where(t => t.DeclaringType != typeof(object)))
		{
			try
			{
				WriteLine($"Testing: {t.Name}");
				System.Reflection.ParameterInfo[] parameters = t.GetParameters();
				foreach (var p in parameters) { WriteLine($"Calling Parameters: {p.ParameterType}"); }
				t.Invoke(unitTests, new object[] { 2, 3 });
				t.Invoke(unitTests, new object[] { 1, 5 });
				t.Invoke(unitTests, new object[] { 3, 2 });
				WriteLine($"{t.Name} ---> Passed!\n");
			}
			catch (Exception ex) { WriteLine(ex.InnerException.Message); }
		}

	}
}
