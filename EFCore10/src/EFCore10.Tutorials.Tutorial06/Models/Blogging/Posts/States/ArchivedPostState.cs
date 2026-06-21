namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class ArchivedPostState : PostState
{
    public const string StateKey = "archived";

    public override string Key => StateKey;

    public override string Name => "Archived";
}
