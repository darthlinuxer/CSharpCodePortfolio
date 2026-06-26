using EFCore10.Tutorials.Tutorial08.Domain;
using EFCore10.Tutorials.Tutorial08.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Application;

internal sealed class StudentProgressReadService(UniversityContext context)
{
    /// <summary>
    /// Returns the SQL used by the student progress projection.
    /// </summary>
    public string GetStudentProgressSql(Semester semester) => BuildStudentProgressRowsQuery(semester).ToQueryString();

    /// <summary>
    /// Reads student semester load, remaining credit points and grades.
    /// </summary>
    public async Task<IReadOnlyList<StudentProgressDto>> GetStudentProgressAsync(
        Semester semester,
        CancellationToken cancellationToken)
    {
        var rows = await BuildStudentProgressRowsQuery(semester)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. rows
            .GroupBy(row => new { row.StudentName, row.Semester })
            .OrderBy(group => group.Key.StudentName.Value, StringComparer.Ordinal)
            .Select(group =>
            {
                var courses = group
                    .OrderBy(row => row.CourseCode.Value, StringComparer.Ordinal)
                    .ToArray();
                var total = courses.Sum(row => row.CreditPoints.Value);

                return new StudentProgressDto(
                    group.Key.StudentName.Value,
                    group.Key.Semester.Value,
                    total,
                    40 - total,
                    AverageFinalGrade(courses),
                    [.. courses.Select(row => new StudentCourseProgressDto(
                        row.CourseCode.Value,
                        row.CourseTitle.Value,
                        row.CreditPoints.Value,
                        row.FinalGrade?.Value))]);
            })];
    }

    private IQueryable<StudentProgressRow> BuildStudentProgressRowsQuery(Semester semester) =>
        context.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.Semester == semester)
            .Select(enrollment => new StudentProgressRow(
                enrollment.Student.Name,
                enrollment.Semester,
                enrollment.Course.Code,
                enrollment.Course.Title,
                enrollment.Course.CreditPoints,
                enrollment.FinalGrade));

    private static decimal? AverageFinalGrade(IReadOnlyCollection<StudentProgressRow> rows)
    {
        var grades = rows
            .Where(row => row.FinalGrade is not null)
            .Select(row => row.FinalGrade!.Value)
            .ToArray();

        return grades is [] ? null : Math.Round(grades.Average(), 2);
    }

    private sealed record StudentProgressRow(
        PersonName StudentName,
        Semester Semester,
        CourseCode CourseCode,
        CourseTitle CourseTitle,
        CreditPoints CreditPoints,
        Grade? FinalGrade);
}
