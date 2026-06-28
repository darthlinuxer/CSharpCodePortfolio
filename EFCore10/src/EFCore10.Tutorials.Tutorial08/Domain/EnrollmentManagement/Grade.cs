namespace EFCore10.Tutorials.Tutorial08.Domain.EnrollmentManagement;

internal sealed record Grade
{
    private Grade(decimal value) => Value = value;

    public decimal Value { get; }

    internal static Result<Grade> Create(decimal value) =>
        value is >= 0m and <= 10m
            ? Result<Grade>.Success(new Grade(value))
            : Result<Grade>.Failure(DomainErrors.GradeInvalid);

    internal static Result<Grade> FromStorage(decimal value) => Create(value);
}
