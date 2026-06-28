namespace EFCore10.Tutorials.Tutorial08.Domain.CourseCatalog;

internal sealed class Course : DomainEntity<CourseId>
{
    private Course()
    {
    }

    private Course(DepartmentId departmentId, CourseTitle title, CourseCode code, CreditPoints creditPoints, Syllabus syllabus)
        : base(CourseId.New())
    {
        DepartmentId = departmentId;
        Title = title;
        Code = code;
        CreditPoints = creditPoints;
        Syllabus = syllabus ?? throw new ArgumentNullException(nameof(syllabus));
    }

    public DepartmentId DepartmentId { get; private set; } = null!;

    public CourseTitle Title { get; private set; } = null!;

    public CourseCode Code { get; private set; } = null!;

    public CreditPoints CreditPoints { get; private set; } = null!;

    public Syllabus Syllabus { get; private set; } = null!;

    public EmployeeId? ProfessorId { get; private set; }

    public static Result<Course> Create(
        DepartmentId departmentId,
        string? title,
        string? code,
        int creditPoints,
        string? syllabusSummary,
        string? syllabusOutcomes)
    {
        ArgumentNullException.ThrowIfNull(departmentId);

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
                departmentId,
                titleResult.RequireValue(),
                codeResult.RequireValue(),
                creditPointsResult.RequireValue(),
                syllabusResult.RequireValue()))
            : Result<Course>.Failure(errors);
    }

    public Result AssignProfessor(ProfessorAssignmentSnapshot professor)
    {
        ArgumentNullException.ThrowIfNull(professor);

        var errors = new List<DomainError>();

        if (professor.Status == EmployeeStatus.Dismissed)
            errors.Add(DomainErrors.ProfessorDismissed);
        if (professor.DepartmentId != DepartmentId)
            errors.Add(DomainErrors.CourseDepartmentMismatch);
        if (ProfessorId is not null && ProfessorId != professor.ProfessorId)
            errors.Add(DomainErrors.CourseProfessorAlreadyAssigned);

        if (errors is not [])
            return Result.Failure(errors);

        ProfessorId = professor.ProfessorId;

        return Result.Success();
    }

    public CourseEnrollmentSnapshot ToEnrollmentSnapshot() => new(Id, CreditPoints);

    public AssignedCourseSnapshot ToAssignedCourseSnapshot() => new(Id, ProfessorId);
}
