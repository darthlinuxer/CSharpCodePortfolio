using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record PersonName
{
    private const int MinLength = 3;
    private const int MaxLength = 200;

    private PersonName(string name, string surname)
    {
        Name = name;
        Surname = surname;
    }

    public string Name { get; }

    public string Surname { get; }

    public string FullName => $"{Name} {Surname}";

    public string Value => FullName;

    public static PersonName Create(string value)
    {
        var fullName = value.NormalizeLength(nameof(PersonName), MinLength, MaxLength);
        var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length == 2
            ? new PersonName(parts[0], parts[1])
            : throw new DomainException("PersonName must include name and surname.");
    }

    public override string ToString() => FullName;
}
