namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct BlogId
{
    private BlogId(Guid value) => Value = value;

    public Guid Value { get; }

    public static BlogId NewId() => new(StronglyTypedId.NewValue());

    public static BlogId From(Guid value) => new(StronglyTypedId.Require(value, "Blog ID"));

    public override string ToString() => Value.ToString();
}
