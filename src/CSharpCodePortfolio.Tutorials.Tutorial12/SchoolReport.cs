namespace CSharpCodePortfolio.Tutorials.Tutorial12;

internal sealed record SchoolReport(
    IReadOnlyList<string> Students,
    IReadOnlyList<string> Teachers,
    string Course,
    IReadOnlyList<string> EnrolledStudents,
    int StudentCount);
