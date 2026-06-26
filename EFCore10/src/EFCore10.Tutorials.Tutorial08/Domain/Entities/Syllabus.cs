namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class Syllabus
{
    private Syllabus()
    {
    }

    /// <summary>
    /// Creates a syllabus owned by a course.
    /// </summary>
    public Syllabus(SyllabusSummary summary, SyllabusOutcomes outcomes)
    {
        Summary = summary;
        Outcomes = outcomes;
    }

    public SyllabusSummary Summary { get; private set; } = null!;

    public SyllabusOutcomes Outcomes { get; private set; } = null!;
}
