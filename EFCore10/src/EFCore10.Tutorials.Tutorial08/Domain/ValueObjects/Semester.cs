namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct Semester(string Value)
{
    internal static Semester Create(int year, int term)
    {
        if (year is < 2000 or > 2100)
            throw new DomainException(DomainErrors.SemesterYearInvalid, "Semester year is outside the supported range.");
        if (term is not (1 or 2))
            throw new DomainException(DomainErrors.SemesterTermInvalid, "Semester term must be 1 or 2.");

        return new Semester($"{year}-{term}");
    }

    internal static Semester FromStorage(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainException(DomainErrors.SemesterRequired, "Semester is required.")
            : new Semester(value);
}
