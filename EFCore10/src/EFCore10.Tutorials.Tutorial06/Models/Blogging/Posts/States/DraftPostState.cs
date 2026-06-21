namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class DraftPostState : PostState
{
    public const string StateKey = "draft";

    public override string Key => StateKey;

    public override string Name => "Draft";

    public override PostState Publish() => new PublishedPostState();
}
