namespace CSharpCodePortfolio.Tutorials.Abstractions;

/// <summary>
/// Marks a class as a tutorial visible in the portfolio host.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class TutorialAttribute(string id, string slug, string title) : Attribute
{
    /// <summary>
    /// Identifier shown in menus.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Command-friendly identifier used by <c>run</c>.
    /// </summary>
    public string Slug { get; } = slug;

    /// <summary>
    /// Human-readable tutorial title.
    /// </summary>
    public string Title { get; } = title;
}
