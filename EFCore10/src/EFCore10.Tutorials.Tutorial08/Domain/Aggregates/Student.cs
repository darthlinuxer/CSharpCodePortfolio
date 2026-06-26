namespace EFCore10.Tutorials.Tutorial08.Domain;

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

    public IReadOnlyCollection<Course> Courses => [.. _enrollments.Select(enrollment => enrollment.Course)];

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

    public Result<Enrollment> RegisterForCourse(Course course, int year, int term, DateTime enrolledAtUtc)
    {
        ArgumentNullException.ThrowIfNull(course);

        var semesterResult = Semester.Create(year, term);
        var enrolledAtResult = UtcDateTime.Create(enrolledAtUtc);
        var errors = new List<DomainError>();
        errors.AddRange(semesterResult.Errors);
        errors.AddRange(enrolledAtResult.Errors);

        if (semesterResult.IsSuccess)
        {
            var semester = semesterResult.RequireValue();
            if (_enrollments.Any(enrollment => enrollment.CourseId == course.Id && enrollment.Semester == semester))
                errors.Add(DomainErrors.StudentAlreadyRegistered);

            var currentPoints = _enrollments
                .Where(enrollment => enrollment.Semester == semester)
                .Sum(enrollment => enrollment.Course.CreditPoints.Value);
            var nextTotal = currentPoints + course.CreditPoints.Value;

            if (nextTotal > MaxCreditPointsPerSemester)
                errors.Add(DomainErrors.StudentCreditLimitExceeded);
        }

        if (errors is not [])
            return Result<Enrollment>.Failure(errors);

        var enrollment = Enrollment.Create(this, course, semesterResult.RequireValue(), enrolledAtResult.RequireValue());
        var addToCourse = course.AddEnrollment(enrollment);

        if (addToCourse.IsFailure)
            return Result<Enrollment>.Failure(addToCourse.Errors);

        _enrollments.Add(enrollment);

        return Result<Enrollment>.Success(enrollment);
    }
}
