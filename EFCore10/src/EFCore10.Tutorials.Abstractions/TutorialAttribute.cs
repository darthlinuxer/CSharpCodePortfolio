namespace EFCore10.Tutorials.Abstractions;

/// <summary>
/// Provides metadata used to discover and display tutorials in the CLI.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TutorialAttribute(string id, string slug, string title) : Attribute
{
    public string Id { get; } = id;

    public string Slug { get; } = slug;

    public string Title { get; } = title;
}
