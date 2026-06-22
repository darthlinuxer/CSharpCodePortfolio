namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class EndedBlogMembershipState : BlogMembershipState
{
    public const string StateKey = "Ended";

    public override string Key => StateKey;

    public override string Name => "Ended";

    public override BlogMembershipState End() => this;
}
