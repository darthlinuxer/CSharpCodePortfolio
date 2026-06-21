using System.Text.RegularExpressions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record PhoneNumber
{
    public PhoneNumber(string value) => Value = value;

    public string Value
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Phone number is required.");

            var normalized = InvalidPhoneChars().Replace(value.Trim(), "");
            field = normalized.Length is >= 8 and <= 20
                ? normalized
                : throw new DomainException("Phone number must have between 8 and 20 characters.");
        }
    }

    public static PhoneNumber Create(string value) => new(value);

    public override string ToString() => Value;

    [GeneratedRegex(@"[^\d+]")]
    private static partial Regex InvalidPhoneChars();
}
