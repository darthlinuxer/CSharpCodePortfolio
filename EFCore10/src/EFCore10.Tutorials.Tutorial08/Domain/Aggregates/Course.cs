namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class Course : DomainEntity<CourseId>
{
    private readonly List<Enrollment> _enrollments = [];

    private Course()
    {
    }

    /// <summary>
    /// Creates a course with an owned syllabus.
    /// </summary>
    public Course(CourseTitle title, CourseCode code, CreditPoints creditPoints, Syllabus syllabus)
        : base(CourseId.New())
    {
        Title = title;
        Code = code;
        CreditPoints = creditPoints;
        Syllabus = syllabus ?? throw new ArgumentNullException(nameof(syllabus));
    }

    public CourseTitle Title { get; private set; }

    public CourseCode Code { get; private set; }

    public CreditPoints CreditPoints { get; private set; }

    public Syllabus Syllabus { get; private set; } = null!;

    public Professor? Professor { get; private set; }

    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments;

    public IReadOnlyCollection<Student> Students => [.. _enrollments.Select(enrollment => enrollment.Student)];

    /// <summary>
    /// Assigns an active professor to the course.
    /// </summary>
    public void AssignProfessor(Professor professor)
    {
        ArgumentNullException.ThrowIfNull(professor);

        if (professor.Status == EmployeeStatus.Dismissed)
            throw new DomainException(DomainErrors.ProfessorDismissed, "Dismissed professors cannot be assigned to courses.");
        if (Professor is not null && Professor.Id != professor.Id)
            throw new DomainException(DomainErrors.CourseProfessorAlreadyAssigned, "Course already has a professor.");

        Professor = professor;
    }
}
