namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct BlogMembershipId
{
    private BlogMembershipId(Guid value) => Value = value;

    public Guid Value { get; }

    public static BlogMembershipId NewId() => new(StronglyTypedId.NewValue());

    public static BlogMembershipId From(Guid value) => new(StronglyTypedId.Require(value, "Blog membership ID"));

    public override string ToString() => Value.ToString();
}
