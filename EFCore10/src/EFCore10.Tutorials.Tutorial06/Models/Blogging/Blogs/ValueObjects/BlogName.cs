namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogName
{
    public BlogName(string value) => Value = value;

    public string Value
    {
        get;
        init => field = PersonName.Normalize(value, nameof(BlogName), 3, 160);
    }

    public static BlogName Create(string value) => new(value);

    public override string ToString() => Value;
}
