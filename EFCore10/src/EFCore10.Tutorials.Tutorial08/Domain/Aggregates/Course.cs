namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class Course : DomainEntity<CourseId>
{
    private readonly List<Enrollment> _enrollments = [];

    private Course()
    {
    }

    private Course(Department department, CourseTitle title, CourseCode code, CreditPoints creditPoints, Syllabus syllabus)
        : base(CourseId.New())
    {
        ArgumentNullException.ThrowIfNull(department);

        Department = department;
        Title = title;
        Code = code;
        CreditPoints = creditPoints;
        Syllabus = syllabus ?? throw new ArgumentNullException(nameof(syllabus));
    }

    public Department Department { get; private set; } = null!;

    public CourseTitle Title { get; private set; } = null!;

    public CourseCode Code { get; private set; } = null!;

    public CreditPoints CreditPoints { get; private set; } = null!;

    public Syllabus Syllabus { get; private set; } = null!;

    public Professor? Professor { get; private set; }

    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments;

    public IReadOnlyCollection<Student> Students => [.. _enrollments.Select(enrollment => enrollment.Student)];

    public static Result<Course> Create(
        Department department,
        string? title,
        string? code,
        int creditPoints,
        string? syllabusSummary,
        string? syllabusOutcomes)
    {
        ArgumentNullException.ThrowIfNull(department);

        var titleResult = CourseTitle.Create(title);
        var codeResult = CourseCode.Create(code);
        var creditPointsResult = CreditPoints.Create(creditPoints);
        var syllabusResult = Syllabus.Create(syllabusSummary, syllabusOutcomes);
        var errors = new List<DomainError>();
        errors.AddRange(titleResult.Errors);
        errors.AddRange(codeResult.Errors);
        errors.AddRange(creditPointsResult.Errors);
        errors.AddRange(syllabusResult.Errors);

        return errors is []
            ? Result<Course>.Success(new Course(
                department,
                titleResult.RequireValue(),
                codeResult.RequireValue(),
                creditPointsResult.RequireValue(),
                syllabusResult.RequireValue()))
            : Result<Course>.Failure(errors);
    }

    public Result AssignProfessor(Professor professor)
    {
        ArgumentNullException.ThrowIfNull(professor);

        var errors = new List<DomainError>();

        if (professor.Status == EmployeeStatus.Dismissed)
            errors.Add(DomainErrors.ProfessorDismissed);
        if (professor.Department.Id != Department.Id)
            errors.Add(DomainErrors.CourseDepartmentMismatch);
        if (Professor is not null && Professor.Id != professor.Id)
            errors.Add(DomainErrors.CourseProfessorAlreadyAssigned);

        if (errors is not [])
            return Result.Failure(errors);

        Professor = professor;

        return Result.Success();
    }

    internal Result AddEnrollment(Enrollment enrollment)
    {
        ArgumentNullException.ThrowIfNull(enrollment);

        var errors = new List<DomainError>();

        if (enrollment.CourseId != Id)
            errors.Add(DomainErrors.EnrollmentCourseMismatch);
        if (_enrollments.Any(value => value.StudentId == enrollment.StudentId && value.Semester == enrollment.Semester))
            errors.Add(DomainErrors.EnrollmentAlreadyAddedToCourse);

        if (errors is not [])
            return Result.Failure(errors);

        _enrollments.Add(enrollment);

        return Result.Success();
    }
}
