namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct BlogMembershipId
{
    private BlogMembershipId(Guid value) => Value = value;

    public Guid Value { get; }

    public static BlogMembershipId NewId() => From(Guid.CreateVersion7());

    public static BlogMembershipId From(Guid value) =>
        value == Guid.Empty ? throw new DomainException("Blog membership ID is required.") : new BlogMembershipId(value);

    public override string ToString() => Value.ToString();
}
