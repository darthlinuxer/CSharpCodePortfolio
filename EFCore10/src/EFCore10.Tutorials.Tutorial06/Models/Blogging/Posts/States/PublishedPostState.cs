namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class PublishedPostState : PostState
{
    public const string StateKey = "published";

    public override string Key => StateKey;

    public override string Name => "Published";

    public override PostState Archive() => new ArchivedPostState();
}
