namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class AuthorBlogMembershipRole : BlogMembershipRole
{
    public const string RoleKey = "Author";

    public override string Key => RoleKey;

    public override string Name => "Author";
}
