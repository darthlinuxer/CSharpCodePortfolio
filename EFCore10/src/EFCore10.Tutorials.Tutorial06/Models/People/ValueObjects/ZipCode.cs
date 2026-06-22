using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record ZipCode
{
    private ZipCode(string value) => Value = value;

    public string Value { get; }

    public static ZipCode Create(string value) => new(Validate(value));

    public override string ToString() => Value;

    private static string Validate(string? value)
    {
        var digits = value.OnlyDigits("ZIP code");

        return digits.Length == 8 ? digits : throw new DomainException("ZIP code must have 8 digits.");
    }
}
