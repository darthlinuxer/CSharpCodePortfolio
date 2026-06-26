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
    public Course(Department department, CourseTitle title, CourseCode code, CreditPoints creditPoints, Syllabus syllabus)
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

    /// <summary>
    /// Assigns an active professor to the course.
    /// </summary>
    public void AssignProfessor(Professor professor)
    {
        ArgumentNullException.ThrowIfNull(professor);

        if (professor.Status == EmployeeStatus.Dismissed)
            throw new DomainException(DomainErrors.ProfessorDismissed, "Dismissed professors cannot be assigned to courses.");
        if (professor.Department.Id != Department.Id)
            throw new DomainException(DomainErrors.CourseDepartmentMismatch, "Course professor must belong to the course department.");
        if (Professor is not null && Professor.Id != professor.Id)
            throw new DomainException(DomainErrors.CourseProfessorAlreadyAssigned, "Course already has a professor.");

        Professor = professor;
    }

    internal void AddEnrollment(Enrollment enrollment)
    {
        ArgumentNullException.ThrowIfNull(enrollment);

        if (enrollment.CourseId != Id)
            throw new DomainException(DomainErrors.EnrollmentCourseMismatch, "Enrollment must belong to this course.");
        if (_enrollments.Any(value => value.StudentId == enrollment.StudentId && value.Semester == enrollment.Semester))
            throw new DomainException(DomainErrors.EnrollmentAlreadyAddedToCourse, "Course already has this enrollment.");

        _enrollments.Add(enrollment);
    }
}
