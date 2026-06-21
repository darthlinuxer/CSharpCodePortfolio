namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostContent
{
    public PostContent(string value) => Value = value;

    public string Value
    {
        get;
        init => field = PersonName.Normalize(value, nameof(PostContent), 1, 4_000);
    }

    public static PostContent Create(string value) => new(value);

    public override string ToString() => Value;
}
