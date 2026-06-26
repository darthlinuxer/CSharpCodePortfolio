namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class Enrollment
{
    private Enrollment()
    {
    }

    private Enrollment(Student student, Course course, Semester semester, UtcDateTime enrolledAtUtc)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(course);

        Student = student;
        Course = course;
        StudentId = student.Id;
        CourseId = course.Id;
        Semester = semester;
        EnrolledAtUtc = enrolledAtUtc;
    }

    public StudentId StudentId { get; private set; } = null!;

    public Student Student { get; private set; } = null!;

    public CourseId CourseId { get; private set; } = null!;

    public Course Course { get; private set; } = null!;

    public Semester Semester { get; private set; } = null!;

    public UtcDateTime EnrolledAtUtc { get; private set; } = null!;

    public Grade? FinalGrade { get; private set; }

    internal static Enrollment Create(Student student, Course course, Semester semester, UtcDateTime enrolledAtUtc) =>
        new(student, course, semester, enrolledAtUtc);

    public Result RecordFinalGrade(decimal grade)
    {
        var gradeResult = Grade.Create(grade);
        var errors = new List<DomainError>(gradeResult.Errors);

        if (FinalGrade is not null)
            errors.Add(DomainErrors.EnrollmentFinalGradeAlreadyRecorded);

        if (errors is not [])
            return Result.Failure(errors);

        FinalGrade = gradeResult.RequireValue();

        return Result.Success();
    }
}
