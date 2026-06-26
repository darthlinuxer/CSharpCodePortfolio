namespace EFCore10.Tutorials.Tutorial08.Domain;

internal static class ProfessorDismissalPolicy
{
    /// <summary>
    /// Protects the cross-aggregate rule for dismissing professors.
    /// </summary>
    public static void EnsureCanDismiss(University university, Employee employee, IReadOnlyCollection<Course> activeCourses)
    {
        ArgumentNullException.ThrowIfNull(university);
        ArgumentNullException.ThrowIfNull(employee);
        ArgumentNullException.ThrowIfNull(activeCourses);

        if (employee.University.Id != university.Id)
            throw new DomainException(DomainErrors.EmployeeUniversityMismatch, "Employee must belong to the university.");

        if (employee is Professor professor
            && activeCourses.Any(course => course.Professor?.Id == professor.Id))
        {
            throw new DomainException(
                DomainErrors.EmployeeDismissalBlocked,
                "Professor cannot be dismissed while assigned to an active course.");
        }
    }
}
