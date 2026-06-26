namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class Enrollment
{
    private Enrollment()
    {
    }

    internal Enrollment(Student student, Course course, Semester semester, UtcDateTime enrolledAtUtc)
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

    public void RecordFinalGrade(Grade grade)
    {
        ArgumentNullException.ThrowIfNull(grade);

        if (FinalGrade is not null)
            throw new DomainException(DomainErrors.EnrollmentFinalGradeAlreadyRecorded, "Final grade is already recorded.");

        FinalGrade = grade;
    }
}
