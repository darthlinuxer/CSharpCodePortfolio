namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class InactiveUserState : UserState
{
    public const string StateKey = "inactive";

    public override string Key => StateKey;

    public override UserState Activate() => new ActiveUserState();
}
