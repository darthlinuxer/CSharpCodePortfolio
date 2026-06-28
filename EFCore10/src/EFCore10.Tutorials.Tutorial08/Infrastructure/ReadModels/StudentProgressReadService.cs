using EFCore10.Tutorials.Tutorial08.Domain;
using EFCore10.Tutorials.Tutorial08.Application.ReadModels;
using EFCore10.Tutorials.Tutorial08.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Infrastructure.ReadModels;

internal sealed class StudentProgressReadService(UniversityContext context) : IStudentProgressReadService
{
    /// <summary>
    /// Returns the SQL used by the student progress projection.
    /// </summary>
    public string GetStudentProgressSql(StudentProgressQuery query) =>
        BuildStudentProgressRowsQuery(ToSemester(query)).ToQueryString();

    /// <summary>
    /// Reads student semester load, remaining credit points and grades.
    /// </summary>
    public async Task<IReadOnlyList<StudentProgressDto>> GetStudentProgressAsync(
        StudentProgressQuery query,
        CancellationToken cancellationToken)
    {
        var semester = ToSemester(query);
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

    private static Semester ToSemester(StudentProgressQuery query) =>
        Semester.Create(query.Year, query.Term).RequireValue();

    private IQueryable<StudentProgressRow> BuildStudentProgressRowsQuery(Semester semester) =>
        from enrollment in context.Enrollments.AsNoTracking()
        where enrollment.Semester == semester
        join student in context.Students.AsNoTracking()
            on enrollment.StudentId equals student.Id
        join course in context.Courses.AsNoTracking()
            on enrollment.CourseId equals course.Id
        select new StudentProgressRow(
            student.Name,
            enrollment.Semester,
            course.Code,
            course.Title,
            course.CreditPoints,
            enrollment.FinalGrade);

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
