namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PostTitle
{
    public PostTitle(string value) => Value = value;

    public string Value
    {
        get;
        init => field = PersonName.Normalize(value, nameof(PostTitle), 3, 200);
    }

    public static PostTitle Create(string value) => new(value);

    public override string ToString() => Value;
}
