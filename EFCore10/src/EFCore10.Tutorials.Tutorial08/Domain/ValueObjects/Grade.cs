namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct Grade(decimal Value)
{
    internal static Grade Create(decimal value) =>
        value is >= 0m and <= 10m
            ? new Grade(value)
            : throw new DomainException(DomainErrors.GradeInvalid, "Final grade must be between 0 and 10.");
}
