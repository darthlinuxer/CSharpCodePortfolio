namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed class EmployeeDirectory
{
    private static readonly Employee[] Employees =
    [
        new(7, "Ada Lovelace", "John 1:1"),
        new(42, "Grace Hopper", "Psalm 23:1")
    ];

    public Employee? FindById(int id) => Employees.SingleOrDefault(employee => employee.Id == id);
}
