namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct PostId
{
    private PostId(Guid value) => Value = value;

    public Guid Value { get; }

    public static PostId NewId() => new(StronglyTypedId.NewValue());

    public static PostId From(Guid value) => new(StronglyTypedId.Require(value, "Post ID"));

    public override string ToString() => Value.ToString();
}
