using EFCore10.Tutorials.Tutorial08.Domain;
using EFCore10.Tutorials.Tutorial08.Application.ReadModels;
using EFCore10.Tutorials.Tutorial08.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Infrastructure.ReadModels;

internal sealed class CourseAnalyticsReadService(UniversityContext context) : ICourseAnalyticsReadService
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
            .Select(enrollment => new CourseGradeRow(enrollment.CourseId, enrollment.FinalGrade))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var gradesByCourse = grades
            .GroupBy(row => row.CourseId)
            .ToDictionary(group => group.Key, group => group.ToArray());

        return [.. rows
            .OrderBy(row => row.CourseCode.Value, StringComparer.Ordinal)
            .Select(row =>
            {
                gradesByCourse.TryGetValue(row.CourseId, out var courseGrades);
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
        from course in context.Courses.AsNoTracking()
        join department in context.Departments.AsNoTracking()
            on course.DepartmentId equals department.Id
        join professor in context.Professors.AsNoTracking()
            on course.ProfessorId equals professor.Id into professors
        from professor in professors.DefaultIfEmpty()
        select new CourseAnalyticsRow(
            course.Id,
            course.Code,
            course.Title,
            course.CreditPoints,
            course.Syllabus.Summary,
            professor == null ? null : professor.Name,
            department.Name,
            context.Enrollments.Count(enrollment => enrollment.CourseId == course.Id),
            context.Enrollments.Count(enrollment => enrollment.CourseId == course.Id && enrollment.FinalGrade != null));

    private static decimal? AverageFinalGrade(IReadOnlyCollection<CourseGradeRow> rows)
    {
        var grades = rows
            .Where(row => row.FinalGrade is not null)
            .Select(row => row.FinalGrade!.Value)
            .ToArray();

        return grades is [] ? null : Math.Round(grades.Average(), 2);
    }

    private sealed record CourseAnalyticsRow(
        CourseId CourseId,
        CourseCode CourseCode,
        CourseTitle CourseTitle,
        CreditPoints CreditPoints,
        SyllabusSummary SyllabusSummary,
        PersonName? ProfessorName,
        DepartmentName? DepartmentName,
        int EnrollmentCount,
        int GradedEnrollmentCount);

    private sealed record CourseGradeRow(CourseId CourseId, Grade? FinalGrade);
}
