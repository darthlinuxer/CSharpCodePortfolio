namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct CreditPoints(int Value)
{
    internal static CreditPoints Create(int value) =>
        value is >= 1 and <= 40
            ? new CreditPoints(value)
            : throw new DomainException(DomainErrors.CreditPointsInvalid, "Course credit points must be between 1 and 40.");
}
