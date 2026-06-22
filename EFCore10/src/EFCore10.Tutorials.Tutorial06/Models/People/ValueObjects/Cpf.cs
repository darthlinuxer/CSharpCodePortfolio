using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record Cpf
{
    private Cpf(string value) => Value = value;

    public string Value { get; }

    public static Cpf Create(string value) => new(Validate(value));

    public override string ToString() => Value;

    private static string Validate(string? value)
    {
        var digits = value.OnlyDigits("CPF");

        return digits.Length == 11 ? digits : throw new DomainException("CPF must have 11 digits.");
    }
}
