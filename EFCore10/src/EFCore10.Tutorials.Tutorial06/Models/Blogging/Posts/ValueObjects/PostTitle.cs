using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostTitle
{
    private PostTitle(string value) => Value = value;

    public string Value { get; }

    public static PostTitle Create(string value) =>
        new(value.NormalizeLength(nameof(PostTitle), 3, 200));

    public override string ToString() => Value;
}
