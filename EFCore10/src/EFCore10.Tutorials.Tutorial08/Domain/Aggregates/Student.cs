namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class Student : DomainEntity<StudentId>
{
    private const int MaxCreditPointsPerSemester = 40;
    private readonly List<Enrollment> _enrollments = [];

    private Student()
    {
    }

    /// <summary>
    /// Creates a student aggregate root.
    /// </summary>
    public Student(PersonName name, EmailAddress email)
        : base(StudentId.New())
    {
        Name = name;
        Email = email;
    }

    public PersonName Name { get; private set; } = null!;

    public EmailAddress Email { get; private set; } = null!;

    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments;

    public IReadOnlyCollection<Course> Courses => [.. _enrollments.Select(enrollment => enrollment.Course)];

    /// <summary>
    /// Registers the student in a course when the semester credit limit allows it.
    /// </summary>
    public Enrollment RegisterForCourse(Course course, Semester semester, UtcDateTime enrolledAtUtc)
    {
        ArgumentNullException.ThrowIfNull(course);

        if (_enrollments.Any(enrollment => enrollment.CourseId == course.Id && enrollment.Semester == semester))
            throw new DomainException(DomainErrors.StudentAlreadyRegistered, "Student is already registered for this course in the semester.");

        var currentPoints = _enrollments
            .Where(enrollment => enrollment.Semester == semester)
            .Sum(enrollment => enrollment.Course.CreditPoints.Value);
        var nextTotal = currentPoints + course.CreditPoints.Value;

        if (nextTotal > MaxCreditPointsPerSemester)
            throw new DomainException(DomainErrors.StudentCreditLimitExceeded, "Student cannot register for more than 40 credit points in the same semester.");

        var enrollment = new Enrollment(this, course, semester, enrolledAtUtc);
        course.AddEnrollment(enrollment);
        _enrollments.Add(enrollment);

        return enrollment;
    }
}
