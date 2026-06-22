namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class ActiveBlogState : BlogState
{
    public override string Key => "active";

    public override string Name => "Active";
}
