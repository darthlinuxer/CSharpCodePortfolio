namespace CSharpCodePortfolio.App.Tutorials;

internal sealed record TutorialDescriptor(
    string Id,
    string Slug,
    string Title,
    Func<CancellationToken, Task> ExecuteAsync)
{
    public bool Matches(string value)
    {
        return string.Equals(Id, value, StringComparison.OrdinalIgnoreCase)
            || string.Equals(Slug, value, StringComparison.OrdinalIgnoreCase);
    }
}
