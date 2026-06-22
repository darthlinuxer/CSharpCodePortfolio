namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct UserId
{
    private UserId(Guid value) => Value = value;

    public Guid Value { get; }

    public static UserId NewId() => new(StronglyTypedId.NewValue());

    public static UserId From(Guid value) => new(StronglyTypedId.Require(value, "User ID"));

    public override string ToString() => Value.ToString();
}
