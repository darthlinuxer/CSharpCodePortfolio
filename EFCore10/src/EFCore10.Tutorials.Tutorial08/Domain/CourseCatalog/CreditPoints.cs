namespace EFCore10.Tutorials.Tutorial08.Domain.CourseCatalog;

internal sealed record CreditPoints
{
    private CreditPoints(int value) => Value = value;

    public int Value { get; }

    internal static Result<CreditPoints> Create(int value) =>
        value is >= 1 and <= 40
            ? Result<CreditPoints>.Success(new CreditPoints(value))
            : Result<CreditPoints>.Failure(DomainErrors.CreditPointsInvalid);

    internal static Result<CreditPoints> FromStorage(int value) => Create(value);
}
