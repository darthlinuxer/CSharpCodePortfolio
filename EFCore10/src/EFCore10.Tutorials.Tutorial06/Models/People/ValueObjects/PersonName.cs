using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record PersonName
{
    private const int MinLength = 3;
    private const int MaxLength = 200;

    private PersonName(string value) => Value = value;

    public string Value { get; }

    public static PersonName Create(string value) =>
        new(value.NormalizeLength(nameof(PersonName), MinLength, MaxLength));

    public override string ToString() => Value;
}
