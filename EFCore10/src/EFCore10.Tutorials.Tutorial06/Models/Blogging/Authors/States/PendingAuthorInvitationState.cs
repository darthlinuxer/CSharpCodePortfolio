namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class PendingAuthorInvitationState : AuthorInvitationState
{
    public override string Key => "pending";

    public override string Name => "Pending";

    public override AuthorInvitationState Accept() => new AcceptedAuthorInvitationState();
}
