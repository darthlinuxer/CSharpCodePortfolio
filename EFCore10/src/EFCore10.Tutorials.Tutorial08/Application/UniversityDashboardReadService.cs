using EFCore10.Tutorials.Tutorial08.Domain;
using EFCore10.Tutorials.Tutorial08.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Application;

internal sealed class UniversityDashboardReadService(UniversityContext context)
{
    /// <summary>
    /// Returns the SQL used by the institutional dashboard projection.
    /// </summary>
    public string GetUniversityDashboardSql() => BuildDashboardQuery().ToQueryString();

    /// <summary>
    /// Reads the institutional dashboard without materializing aggregate graphs.
    /// </summary>
    public async Task<UniversityDashboardDto> GetUniversityDashboardAsync(CancellationToken cancellationToken)
    {
        var row = await BuildDashboardQuery()
            .SingleAsync(cancellationToken)
            .ConfigureAwait(false);

        return new UniversityDashboardDto(
            row.UniversityName.Value,
            row.CampusCount,
            row.DepartmentCount,
            row.EmployeeCount,
            row.ProfessorCount,
            row.AdministrativeEmployeeCount,
            row.ActiveEmployeeCount,
            row.CourseCount,
            row.StudentCount,
            row.EnrollmentCount);
    }

    private IQueryable<UniversityDashboardRow> BuildDashboardQuery() =>
        context.Universities
            .AsNoTracking()
            .Select(university => new UniversityDashboardRow(
                university.Name,
                university.Campuses.Count,
                university.Departments.Count,
                university.Employees.Count,
                context.Professors.Count(),
                context.AdministrativeEmployees.Count(),
                context.Employees.Count(employee => employee.Status == EmployeeStatus.Active),
                context.Courses.Count(),
                context.Students.Count(),
                context.Enrollments.Count()));

    private sealed record UniversityDashboardRow(
        UniversityName UniversityName,
        int CampusCount,
        int DepartmentCount,
        int EmployeeCount,
        int ProfessorCount,
        int AdministrativeEmployeeCount,
        int ActiveEmployeeCount,
        int CourseCount,
        int StudentCount,
        int EnrollmentCount);
}
