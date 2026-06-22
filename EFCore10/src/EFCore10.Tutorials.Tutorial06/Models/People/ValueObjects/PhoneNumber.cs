using System.Text.RegularExpressions;
using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record PhoneNumber
{
    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    public static PhoneNumber Create(string value) => new(Validate(value));

    public override string ToString() => Value;

    private static string Validate(string? value)
    {
        var normalized = InvalidPhoneChars().Replace(value.TrimRequired("Phone number"), "");

        return normalized.Length is >= 8 and <= 20
            ? normalized
            : throw new DomainException("Phone number must have between 8 and 20 characters.");
    }

    [GeneratedRegex(@"[^\d+]")]
    private static partial Regex InvalidPhoneChars();
}
