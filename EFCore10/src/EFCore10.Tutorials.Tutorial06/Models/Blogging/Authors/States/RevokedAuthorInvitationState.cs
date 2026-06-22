namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class RevokedAuthorInvitationState : AuthorInvitationState
{
    public override string Key => "revoked";

    public override string Name => "Revoked";

    public override AuthorInvitationState Revoke() => this;
}
