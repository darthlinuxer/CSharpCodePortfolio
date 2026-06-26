namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class University : DomainEntity<UniversityId>
{
    private readonly List<UniversityCampus> _campuses = [];
    private readonly List<Department> _departments = [];
    private readonly List<Employee> _employees = [];

    private University()
    {
    }

    /// <summary>
    /// Creates a university aggregate root.
    /// </summary>
    public University(UniversityName name)
        : base(UniversityId.New())
    {
        Name = name;
    }

    public UniversityName Name { get; private set; } = null!;

    public IReadOnlyCollection<UniversityCampus> Campuses => _campuses;

    public IReadOnlyCollection<Department> Departments => _departments;

    public IReadOnlyCollection<Employee> Employees => _employees;

    /// <summary>
    /// Adds a campus owned by the university.
    /// </summary>
    public void AddCampus(CampusName name, CityName city)
    {
        if (_campuses.Any(campus => string.Equals(campus.Name.Value, name.Value, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException(DomainErrors.CampusNameDuplicated, "University campus names must be unique.");

        _campuses.Add(new UniversityCampus(CampusId.Create(_campuses.Count + 1), name, city));
    }

    /// <summary>
    /// Creates a department that belongs to this university.
    /// </summary>
    public Department OpenDepartment(DepartmentName name)
    {
        if (_departments.Any(department => string.Equals(department.Name.Value, name.Value, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException(DomainErrors.DepartmentNameDuplicated, "Department names must be unique inside the university.");

        var department = new Department(name, this);
        _departments.Add(department);

        return department;
    }

    /// <summary>
    /// Hires a professor for a department without assigning a course.
    /// </summary>
    public Professor HireProfessor(
        PersonName name,
        EmailAddress email,
        Department department,
        UtcDateTime hiredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(department);

        if (department.University.Id != Id)
            throw new DomainException(DomainErrors.ProfessorDepartmentMismatch, "Professor department must belong to the same university.");

        var professor = new Professor(name, email, this, department, hiredAtUtc);
        _employees.Add(professor);
        department.AddProfessor(professor);

        return professor;
    }

    /// <summary>
    /// Hires a non-teaching employee for the university.
    /// </summary>
    public AdministrativeEmployee HireAdministrativeEmployee(
        PersonName name,
        EmailAddress email,
        StaffRole role,
        UtcDateTime hiredAtUtc)
    {
        var employee = new AdministrativeEmployee(name, email, this, role, hiredAtUtc);
        _employees.Add(employee);

        return employee;
    }

    /// <summary>
    /// Dismisses an employee unless a professor still owns an active course assignment.
    /// </summary>
    public void DismissEmployee(Employee employee, UtcDateTime dismissedAtUtc, IReadOnlyCollection<Course> activeCourses)
    {
        ArgumentNullException.ThrowIfNull(employee);
        ArgumentNullException.ThrowIfNull(activeCourses);

        ProfessorDismissalPolicy.EnsureCanDismiss(this, employee, activeCourses);

        employee.Dismiss(dismissedAtUtc);
    }
}
