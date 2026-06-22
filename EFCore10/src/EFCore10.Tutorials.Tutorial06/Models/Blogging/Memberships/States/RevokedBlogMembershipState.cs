namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class RevokedBlogMembershipState : BlogMembershipState
{
    public const string StateKey = "Revoked";

    public override string Key => StateKey;

    public override string Name => "Revoked";

    public override BlogMembershipState Revoke() => this;

    public override BlogMembershipState End() => this;
}
