namespace EFCore10.Tutorials.Tutorial08.Domain.EnrollmentManagement;

internal sealed class Student : DomainEntity<StudentId>
{
    private const int MaxCreditPointsPerSemester = 40;
    private readonly List<Enrollment> _enrollments = [];

    private Student()
    {
    }

    private Student(PersonName name, EmailAddress email)
        : base(StudentId.New())
    {
        Name = name;
        Email = email;
    }

    public PersonName Name { get; private set; } = null!;

    public EmailAddress Email { get; private set; } = null!;

    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments;

    public static Result<Student> Create(string? name, string? email)
    {
        var nameResult = PersonName.Create(name);
        var emailResult = EmailAddress.Create(email);
        var errors = new List<DomainError>();
        errors.AddRange(nameResult.Errors);
        errors.AddRange(emailResult.Errors);

        return errors is []
            ? Result<Student>.Success(new Student(nameResult.RequireValue(), emailResult.RequireValue()))
            : Result<Student>.Failure(errors);
    }

    public Result<Enrollment> RegisterForCourse(
        CourseEnrollmentSnapshot course,
        IReadOnlyCollection<CourseEnrollmentSnapshot> currentSemesterCourses,
        int year,
        int term,
        DateTime enrolledAtUtc)
    {
        ArgumentNullException.ThrowIfNull(course);
        ArgumentNullException.ThrowIfNull(currentSemesterCourses);

        var semesterResult = Semester.Create(year, term);
        var enrolledAtResult = UtcDateTime.Create(enrolledAtUtc);
        var errors = new List<DomainError>();
        errors.AddRange(semesterResult.Errors);
        errors.AddRange(enrolledAtResult.Errors);

        if (semesterResult.IsSuccess)
        {
            var semester = semesterResult.RequireValue();
            if (_enrollments.Any(enrollment => enrollment.CourseId == course.CourseId && enrollment.Semester == semester))
                errors.Add(DomainErrors.StudentAlreadyRegistered);

            var currentPoints = currentSemesterCourses.Sum(value => value.CreditPoints.Value);
            var nextTotal = currentPoints + course.CreditPoints.Value;

            if (nextTotal > MaxCreditPointsPerSemester)
                errors.Add(DomainErrors.StudentCreditLimitExceeded);
        }

        if (errors is not [])
            return Result<Enrollment>.Failure(errors);

        var enrollment = Enrollment.Create(Id, course.CourseId, semesterResult.RequireValue(), enrolledAtResult.RequireValue());
        _enrollments.Add(enrollment);

        return Result<Enrollment>.Success(enrollment);
    }
}
