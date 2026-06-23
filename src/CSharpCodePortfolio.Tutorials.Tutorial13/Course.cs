namespace CSharpCodePortfolio.Tutorials.Tutorial13;

internal sealed class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int TeacherId { get; set; }

    public Teacher? Teacher { get; set; }

    public List<Student> Students { get; } = [];
}
