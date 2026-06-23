namespace CSharpCodePortfolio.Tutorials.Tutorial13;

internal sealed record SchoolReport(
    IReadOnlyList<string> Students,
    IReadOnlyList<string> Teachers,
    string Course,
    IReadOnlyList<string> EnrolledStudents,
    int StudentCount);
