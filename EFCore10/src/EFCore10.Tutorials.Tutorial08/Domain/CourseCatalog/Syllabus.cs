namespace EFCore10.Tutorials.Tutorial08.Domain.CourseCatalog;

internal sealed class Syllabus
{
    private Syllabus()
    {
    }

    private Syllabus(SyllabusSummary summary, SyllabusOutcomes outcomes)
    {
        Summary = summary;
        Outcomes = outcomes;
    }

    public SyllabusSummary Summary { get; private set; } = null!;

    public SyllabusOutcomes Outcomes { get; private set; } = null!;

    public static Result<Syllabus> Create(string? summary, string? outcomes)
    {
        var summaryResult = SyllabusSummary.Create(summary);
        var outcomesResult = SyllabusOutcomes.Create(outcomes);
        var errors = new List<DomainError>();
        errors.AddRange(summaryResult.Errors);
        errors.AddRange(outcomesResult.Errors);

        return errors is []
            ? Result<Syllabus>.Success(new Syllabus(summaryResult.RequireValue(), outcomesResult.RequireValue()))
            : Result<Syllabus>.Failure(errors);
    }
}
