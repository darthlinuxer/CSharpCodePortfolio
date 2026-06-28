namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal static class ProfessorDismissalPolicy
{
    /// <summary>
    /// Protects the cross-aggregate rule for dismissing professors.
    /// </summary>
    public static Result EnsureCanDismiss(University university, Employee employee, IReadOnlyCollection<AssignedCourseSnapshot> activeCourses)
    {
        ArgumentNullException.ThrowIfNull(university);
        ArgumentNullException.ThrowIfNull(employee);
        ArgumentNullException.ThrowIfNull(activeCourses);

        var errors = new List<DomainError>();

        if (employee.University.Id != university.Id)
            errors.Add(DomainErrors.EmployeeUniversityMismatch);

        if (employee is Professor professor
            && activeCourses.Any(course => course.ProfessorId == professor.Id))
        {
            errors.Add(DomainErrors.EmployeeDismissalBlocked);
        }

        return errors is [] ? Result.Success() : Result.Failure(errors);
    }
}
