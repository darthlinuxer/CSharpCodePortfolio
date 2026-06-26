namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class Enrollment
{
    private Enrollment()
    {
    }

    internal Enrollment(Student student, Course course, Semester semester, UtcDateTime enrolledAtUtc, Grade? finalGrade)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(course);

        Student = student;
        Course = course;
        StudentId = student.Id;
        CourseId = course.Id;
        Semester = semester;
        EnrolledAtUtc = enrolledAtUtc;
        FinalGrade = finalGrade;
    }

    public StudentId StudentId { get; private set; }

    public Student Student { get; private set; } = null!;

    public CourseId CourseId { get; private set; }

    public Course Course { get; private set; } = null!;

    public Semester Semester { get; private set; }

    public UtcDateTime EnrolledAtUtc { get; private set; }

    public Grade? FinalGrade { get; private set; }
}
