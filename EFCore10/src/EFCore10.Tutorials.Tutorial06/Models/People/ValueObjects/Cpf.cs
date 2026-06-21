using System.Text.RegularExpressions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record Cpf
{
    public Cpf(string value) => Value = value;

    public string Value
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("CPF is required.");

            var digits = NonDigits().Replace(value, "");
            field = digits.Length == 11 ? digits : throw new DomainException("CPF must have 11 digits.");
        }
    }

    public static Cpf Create(string value) => new(value);

    public override string ToString() => Value;

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigits();
}
