using EFCore10.Tutorials.Tutorial08.Domain;
using EFCore10.Tutorials.Tutorial08.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Application;

internal sealed class FacultyWorkloadReadService(UniversityContext context)
{
    /// <summary>
    /// Returns the SQL used by the faculty workload projection.
    /// </summary>
    public string GetFacultyWorkloadSql() => BuildFacultyWorkloadRowsQuery().ToQueryString();

    /// <summary>
    /// Reads professor workload grouped by professor and department.
    /// </summary>
    public async Task<IReadOnlyList<FacultyWorkloadDto>> GetFacultyWorkloadAsync(CancellationToken cancellationToken)
    {
        var rows = await BuildFacultyWorkloadRowsQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. rows
            .GroupBy(row => new { row.ProfessorName, row.DepartmentName, row.Status })
            .OrderBy(group => group.Key.ProfessorName.Value, StringComparer.Ordinal)
            .Select(group =>
            {
                var courses = group
                    .OrderBy(row => row.CourseCode.Value, StringComparer.Ordinal)
                    .ToArray();

                return new FacultyWorkloadDto(
                    group.Key.ProfessorName.Value,
                    group.Key.DepartmentName.Value,
                    group.Key.Status.Value,
                    courses.Length,
                    courses.Sum(row => row.CreditPoints.Value),
                    courses.Sum(row => row.EnrollmentCount),
                    [.. courses.Select(row => new FacultyCourseWorkloadDto(
                        row.CourseCode.Value,
                        row.CourseTitle.Value,
                        row.CreditPoints.Value,
                        row.EnrollmentCount))]);
            })];
    }

    private IQueryable<FacultyWorkloadRow> BuildFacultyWorkloadRowsQuery() =>
        context.Courses
            .AsNoTracking()
            .Where(course => course.Professor != null)
            .Select(course => new FacultyWorkloadRow(
                course.Professor!.Name,
                course.Professor.Department.Name,
                course.Professor.Status,
                course.Code,
                course.Title,
                course.CreditPoints,
                course.Enrollments.Count));

    private sealed record FacultyWorkloadRow(
        PersonName ProfessorName,
        DepartmentName DepartmentName,
        EmployeeStatus Status,
        CourseCode CourseCode,
        CourseTitle CourseTitle,
        CreditPoints CreditPoints,
        int EnrollmentCount);
}
