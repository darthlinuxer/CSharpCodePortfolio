namespace CSharpCodePortfolio.Tutorials.Tutorial13;

internal sealed class Teacher
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<Course> Courses { get; } = [];
}
