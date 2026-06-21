namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class LockedUserState : UserState
{
    public const string StateKey = "locked";

    public override string Key => StateKey;

    public override UserState Lock() => this;
}
