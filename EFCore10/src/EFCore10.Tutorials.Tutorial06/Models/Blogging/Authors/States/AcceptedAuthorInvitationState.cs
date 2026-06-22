namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class AcceptedAuthorInvitationState : AuthorInvitationState
{
    public override string Key => "accepted";

    public override string Name => "Accepted";

    public override bool CanPost => true;

    public override AuthorInvitationState Accept() => this;
}
