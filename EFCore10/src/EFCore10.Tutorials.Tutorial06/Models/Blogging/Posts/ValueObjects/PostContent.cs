using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostContent
{
    private PostContent(string value) => Value = value;

    public string Value { get; }

    public static PostContent Create(string value) =>
        new(value.NormalizeLength(nameof(PostContent), 1, 4_000));

    public override string ToString() => Value;
}
