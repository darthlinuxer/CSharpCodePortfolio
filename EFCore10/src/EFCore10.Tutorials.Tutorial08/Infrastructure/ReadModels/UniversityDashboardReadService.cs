using EFCore10.Tutorials.Tutorial08.Domain;
using EFCore10.Tutorials.Tutorial08.Application.ReadModels;
using EFCore10.Tutorials.Tutorial08.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Infrastructure.ReadModels;

internal sealed class UniversityDashboardReadService(UniversityContext context) : IUniversityDashboardReadService
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
                context.Professors.Count(professor => professor.University.Id == university.Id),
                context.AdministrativeEmployees.Count(employee => employee.University.Id == university.Id),
                context.Employees.Count(employee => employee.University.Id == university.Id && employee.Status == EmployeeStatus.Active),
                context.Courses.Count(course => context.Departments.Any(department => department.Id == course.DepartmentId && department.University.Id == university.Id)),
                context.Enrollments
                    .Where(enrollment => context.Courses.Any(course =>
                        course.Id == enrollment.CourseId
                        && context.Departments.Any(department => department.Id == course.DepartmentId && department.University.Id == university.Id)))
                    .Select(enrollment => enrollment.StudentId)
                    .Distinct()
                    .Count(),
                context.Enrollments.Count(enrollment => context.Courses.Any(course =>
                    course.Id == enrollment.CourseId
                    && context.Departments.Any(department => department.Id == course.DepartmentId && department.University.Id == university.Id)))));

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
