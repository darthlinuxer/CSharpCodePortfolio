using System.Text.RegularExpressions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record ZipCode
{
    public ZipCode(string value) => Value = value;

    public string Value
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("ZIP code is required.");

            var digits = NonDigits().Replace(value, "");
            field = digits.Length == 8 ? digits : throw new DomainException("ZIP code must have 8 digits.");
        }
    }

    public static ZipCode Create(string value) => new(value);

    public override string ToString() => Value;

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigits();
}
