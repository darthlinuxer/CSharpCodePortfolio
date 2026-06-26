namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record Semester
{
    private Semester(string value) => Value = value;

    public string Value { get; }

    internal static Semester Create(int year, int term)
    {
        if (year is < 2000 or > 2100)
            throw new DomainException(DomainErrors.SemesterYearInvalid, "Semester year is outside the supported range.");
        if (term is not (1 or 2))
            throw new DomainException(DomainErrors.SemesterTermInvalid, "Semester term must be 1 or 2.");

        return new Semester($"{year}-{term}");
    }

    internal static Semester FromStorage(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.SemesterRequired, "Semester is required.");

        var parts = value.Split('-', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var year))
            throw new DomainException(DomainErrors.SemesterYearInvalid, "Semester year is invalid.");
        if (!int.TryParse(parts[1], out var term))
            throw new DomainException(DomainErrors.SemesterTermInvalid, "Semester term is invalid.");

        return Create(year, term);
    }
}
