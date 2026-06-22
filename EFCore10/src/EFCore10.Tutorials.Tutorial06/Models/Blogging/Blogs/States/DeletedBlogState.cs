namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class DeletedBlogState : BlogState
{
    public override string Key => "deleted";

    public override string Name => "Deleted";

    public override bool AllowsChanges => false;

    public override BlogState Delete() => this;
}
