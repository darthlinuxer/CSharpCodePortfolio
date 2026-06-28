namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal sealed class University : DomainEntity<UniversityId>
{
    private readonly List<UniversityCampus> _campuses = [];
    private readonly List<Department> _departments = [];
    private readonly List<Employee> _employees = [];

    private University()
    {
    }

    private University(UniversityName name)
        : base(UniversityId.New())
    {
        Name = name;
    }

    public UniversityName Name { get; private set; } = null!;

    public IReadOnlyCollection<UniversityCampus> Campuses => _campuses;

    public IReadOnlyCollection<Department> Departments => _departments;

    public IReadOnlyCollection<Employee> Employees => _employees;

    public static Result<University> Create(string? name)
    {
        var nameResult = UniversityName.Create(name);

        return nameResult.IsSuccess
            ? Result<University>.Success(new University(nameResult.RequireValue()))
            : Result<University>.Failure(nameResult.Errors);
    }

    public Result AddCampus(string? name, string? city)
    {
        var nameResult = CampusName.Create(name);
        var cityResult = CityName.Create(city);
        var errors = new List<DomainError>();
        errors.AddRange(nameResult.Errors);
        errors.AddRange(cityResult.Errors);

        if (nameResult.IsSuccess
            && _campuses.Any(campus => string.Equals(campus.Name.Value, nameResult.RequireValue().Value, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add(DomainErrors.CampusNameDuplicated);
        }

        if (errors is not [])
            return Result.Failure(errors);

        _campuses.Add(UniversityCampus.Create(
            CampusId.Create(_campuses.Count + 1).RequireValue(),
            nameResult.RequireValue(),
            cityResult.RequireValue()));

        return Result.Success();
    }

    public Result<Department> OpenDepartment(string? name)
    {
        var nameResult = DepartmentName.Create(name);
        var errors = new List<DomainError>(nameResult.Errors);

        if (nameResult.IsSuccess
            && _departments.Any(department => string.Equals(department.Name.Value, nameResult.RequireValue().Value, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add(DomainErrors.DepartmentNameDuplicated);
        }

        if (errors is not [])
            return Result<Department>.Failure(errors);

        var department = Department.Create(nameResult.RequireValue(), this);
        _departments.Add(department);

        return Result<Department>.Success(department);
    }

    public Result<Professor> HireProfessor(
        string? name,
        string? email,
        Department department,
        DateTime hiredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(department);

        var nameResult = PersonName.Create(name);
        var emailResult = EmailAddress.Create(email);
        var hiredAtResult = UtcDateTime.Create(hiredAtUtc);
        var errors = new List<DomainError>();
        errors.AddRange(nameResult.Errors);
        errors.AddRange(emailResult.Errors);
        errors.AddRange(hiredAtResult.Errors);

        if (department.University.Id != Id)
            errors.Add(DomainErrors.ProfessorDepartmentMismatch);

        if (errors is not [])
            return Result<Professor>.Failure(errors);

        var professor = Professor.Create(
            nameResult.RequireValue(),
            emailResult.RequireValue(),
            this,
            department,
            hiredAtResult.RequireValue());
        _employees.Add(professor);
        department.AddProfessor(professor);

        return Result<Professor>.Success(professor);
    }

    public Result<AdministrativeEmployee> HireAdministrativeEmployee(
        string? name,
        string? email,
        string? role,
        DateTime hiredAtUtc)
    {
        var nameResult = PersonName.Create(name);
        var emailResult = EmailAddress.Create(email);
        var roleResult = StaffRole.Create(role);
        var hiredAtResult = UtcDateTime.Create(hiredAtUtc);
        var errors = new List<DomainError>();
        errors.AddRange(nameResult.Errors);
        errors.AddRange(emailResult.Errors);
        errors.AddRange(roleResult.Errors);
        errors.AddRange(hiredAtResult.Errors);

        if (errors is not [])
            return Result<AdministrativeEmployee>.Failure(errors);

        var employee = AdministrativeEmployee.Create(
            nameResult.RequireValue(),
            emailResult.RequireValue(),
            this,
            roleResult.RequireValue(),
            hiredAtResult.RequireValue());
        _employees.Add(employee);

        return Result<AdministrativeEmployee>.Success(employee);
    }

    public Result DismissEmployee(Employee employee, DateTime dismissedAtUtc, IReadOnlyCollection<AssignedCourseSnapshot> activeCourses)
    {
        ArgumentNullException.ThrowIfNull(employee);
        ArgumentNullException.ThrowIfNull(activeCourses);

        var dismissedAtResult = UtcDateTime.Create(dismissedAtUtc);
        var policyResult = ProfessorDismissalPolicy.EnsureCanDismiss(this, employee, activeCourses);
        var errors = new List<DomainError>();
        errors.AddRange(dismissedAtResult.Errors);
        errors.AddRange(policyResult.Errors);

        if (dismissedAtResult.IsSuccess)
            errors.AddRange(employee.GetDismissalErrors(dismissedAtResult.RequireValue()));

        if (errors is not [])
            return Result.Failure(errors);

        employee.Dismiss(dismissedAtResult.RequireValue()).RequireSuccess();

        return Result.Success();
    }
}
