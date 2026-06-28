namespace EFCore10.Tutorials.Tutorial08.Domain.EnrollmentManagement;

internal sealed class Enrollment
{
    private Enrollment()
    {
    }

    private Enrollment(StudentId studentId, CourseId courseId, Semester semester, UtcDateTime enrolledAtUtc)
    {
        StudentId = studentId;
        CourseId = courseId;
        Semester = semester;
        EnrolledAtUtc = enrolledAtUtc;
    }

    public StudentId StudentId { get; private set; } = null!;

    public CourseId CourseId { get; private set; } = null!;

    public Semester Semester { get; private set; } = null!;

    public UtcDateTime EnrolledAtUtc { get; private set; } = null!;

    public Grade? FinalGrade { get; private set; }

    internal static Enrollment Create(StudentId studentId, CourseId courseId, Semester semester, UtcDateTime enrolledAtUtc) =>
        new(studentId, courseId, semester, enrolledAtUtc);

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
