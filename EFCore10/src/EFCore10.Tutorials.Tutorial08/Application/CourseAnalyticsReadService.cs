using EFCore10.Tutorials.Tutorial08.Domain;
using EFCore10.Tutorials.Tutorial08.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Application;

internal sealed class CourseAnalyticsReadService(UniversityContext context)
{
    /// <summary>
    /// Returns the SQL used by the course demand and grade projection.
    /// </summary>
    public string GetCourseAnalyticsSql() => BuildCourseAnalyticsQuery().ToQueryString();

    /// <summary>
    /// Reads demand, professor and grade metrics by course.
    /// </summary>
    public async Task<IReadOnlyList<CourseAnalyticsDto>> GetCourseAnalyticsAsync(CancellationToken cancellationToken)
    {
        var rows = await BuildCourseAnalyticsQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var grades = await context.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.FinalGrade != null)
            .Select(enrollment => new CourseGradeRow(enrollment.Course.Code, enrollment.FinalGrade))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var gradesByCourse = grades
            .GroupBy(row => row.CourseCode)
            .ToDictionary(group => group.Key, group => group.ToArray());

        return [.. rows
            .OrderBy(row => row.CourseCode.Value, StringComparer.Ordinal)
            .Select(row =>
            {
                gradesByCourse.TryGetValue(row.CourseCode, out var courseGrades);
                courseGrades ??= [];

                return new CourseAnalyticsDto(
                    row.CourseCode.Value,
                    row.CourseTitle.Value,
                    row.CreditPoints.Value,
                    row.SyllabusSummary.Value,
                    row.ProfessorName?.Value,
                    row.DepartmentName?.Value,
                    row.EnrollmentCount,
                    row.GradedEnrollmentCount,
                    AverageFinalGrade(courseGrades));
            })];
    }

    private IQueryable<CourseAnalyticsRow> BuildCourseAnalyticsQuery() =>
        context.Courses
            .AsNoTracking()
            .Select(course => new CourseAnalyticsRow(
                course.Code,
                course.Title,
                course.CreditPoints,
                course.Syllabus.Summary,
                course.Professor == null ? null : course.Professor.Name,
                course.Department.Name,
                course.Enrollments.Count,
                course.Enrollments.Count(enrollment => enrollment.FinalGrade != null)));

    private static decimal? AverageFinalGrade(IReadOnlyCollection<CourseGradeRow> rows)
    {
        var grades = rows
            .Where(row => row.FinalGrade is not null)
            .Select(row => row.FinalGrade!.Value)
            .ToArray();

        return grades is [] ? null : Math.Round(grades.Average(), 2);
    }

    private sealed record CourseAnalyticsRow(
        CourseCode CourseCode,
        CourseTitle CourseTitle,
        CreditPoints CreditPoints,
        SyllabusSummary SyllabusSummary,
        PersonName? ProfessorName,
        DepartmentName? DepartmentName,
        int EnrollmentCount,
        int GradedEnrollmentCount);

    private sealed record CourseGradeRow(CourseCode CourseCode, Grade? FinalGrade);
}
