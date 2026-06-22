namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class ActiveBlogMembershipState : BlogMembershipState
{
    public const string StateKey = "Active";

    public override string Key => StateKey;

    public override string Name => "Active";

    public override bool IsActive => true;

    public override bool CanPost => true;

    public override BlogMembershipState Activate() => this;
}
