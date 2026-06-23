namespace CSharpCodePortfolio.Tutorials.Tutorial13;

internal sealed class Student
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<Course> Courses { get; } = [];
}
