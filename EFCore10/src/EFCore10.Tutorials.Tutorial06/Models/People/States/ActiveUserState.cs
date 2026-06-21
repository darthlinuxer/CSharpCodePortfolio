namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class ActiveUserState : UserState
{
    public const string StateKey = "active";

    public override string Key => StateKey;

    public override bool CanLogin => true;

    public override UserState Deactivate() => new InactiveUserState();
}
