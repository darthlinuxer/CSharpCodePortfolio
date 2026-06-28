namespace EFCore10.Tutorials.Tutorial08.Domain.EnrollmentManagement;

internal sealed record Semester
{
    private Semester(string value) => Value = value;

    public string Value { get; }

    internal static Result<Semester> Create(int year, int term)
    {
        var errors = new List<DomainError>();

        if (year is < 2000 or > 2100)
            errors.Add(DomainErrors.SemesterYearInvalid);
        if (term is not (1 or 2))
            errors.Add(DomainErrors.SemesterTermInvalid);

        return errors is []
            ? Result<Semester>.Success(new Semester($"{year}-{term}"))
            : Result<Semester>.Failure(errors);
    }

    internal static Result<Semester> FromStorage(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Semester>.Failure(DomainErrors.SemesterRequired);

        var parts = value.Split('-', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var year))
            return Result<Semester>.Failure(DomainErrors.SemesterYearInvalid);
        if (!int.TryParse(parts[1], out var term))
            return Result<Semester>.Failure(DomainErrors.SemesterTermInvalid);

        return Create(year, term);
    }
}
