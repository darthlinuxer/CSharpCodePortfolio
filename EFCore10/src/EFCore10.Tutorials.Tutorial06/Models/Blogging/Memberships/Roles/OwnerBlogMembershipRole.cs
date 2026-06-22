namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class OwnerBlogMembershipRole : BlogMembershipRole
{
    public const string RoleKey = "Owner";

    public override string Key => RoleKey;

    public override string Name => "Owner";
}
