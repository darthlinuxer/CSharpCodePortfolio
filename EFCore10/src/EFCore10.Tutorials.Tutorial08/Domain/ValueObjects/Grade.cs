namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record Grade
{
    private Grade(decimal value) => Value = value;

    public decimal Value { get; }

    internal static Grade Create(decimal value) =>
        value is >= 0m and <= 10m
            ? new Grade(value)
            : throw new DomainException(DomainErrors.GradeInvalid, "Final grade must be between 0 and 10.");

    internal static Grade FromStorage(decimal value) => Create(value);
}
