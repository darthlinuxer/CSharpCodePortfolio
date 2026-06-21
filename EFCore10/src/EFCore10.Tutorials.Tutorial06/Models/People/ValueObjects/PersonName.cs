namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record PersonName
{
    private const int MinLength = 3;
    private const int MaxLength = 200;

    public PersonName(string value) => Value = value;

    public string Value
    {
        get;
        init => field = Normalize(value, nameof(PersonName), MinLength, MaxLength);
    }

    public static PersonName Create(string value) => new(value);

    public override string ToString() => Value;

    internal static string Normalize(string? value, string valueObjectName, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{valueObjectName} is required.");

        var normalized = value.Trim();

        return normalized.Length < minLength || normalized.Length > maxLength
            ? throw new DomainException($"{valueObjectName} must have between {minLength} and {maxLength} characters.")
            : normalized;
    }
}
