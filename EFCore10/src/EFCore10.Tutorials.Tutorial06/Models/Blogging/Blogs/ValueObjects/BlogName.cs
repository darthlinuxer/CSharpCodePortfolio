using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogName
{
    private BlogName(string value) => Value = value;

    public string Value { get; }

    public static BlogName Create(string value) =>
        new(value.NormalizeLength(nameof(BlogName), 3, 160));

    public override string ToString() => Value;
}
