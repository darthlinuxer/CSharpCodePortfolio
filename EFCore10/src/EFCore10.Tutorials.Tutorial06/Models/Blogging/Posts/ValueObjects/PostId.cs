namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct PostId
{
    private PostId(Guid value) => Value = value;

    public Guid Value { get; }

    public static PostId NewId() => From(Guid.CreateVersion7());

    public static PostId From(Guid value) =>
        value == Guid.Empty ? throw new DomainException("Post ID is required.") : new PostId(value);

    public override string ToString() => Value.ToString();
}
