namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record CreditPoints
{
    private CreditPoints(int value) => Value = value;

    public int Value { get; }

    internal static CreditPoints Create(int value) =>
        value is >= 1 and <= 40
            ? new CreditPoints(value)
            : throw new DomainException(DomainErrors.CreditPointsInvalid, "Course credit points must be between 1 and 40.");

    internal static CreditPoints FromStorage(int value) => Create(value);
}
