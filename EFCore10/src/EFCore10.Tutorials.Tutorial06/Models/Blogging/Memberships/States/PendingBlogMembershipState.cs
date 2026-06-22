namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class PendingBlogMembershipState : BlogMembershipState
{
    public const string StateKey = "Pending";

    public override string Key => StateKey;

    public override string Name => "Pending";

    public override BlogMembershipState Activate() => new ActiveBlogMembershipState();
}
