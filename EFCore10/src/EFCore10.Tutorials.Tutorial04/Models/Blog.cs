namespace EFCore10.Tutorials.Tutorial04.Models;

public class Blog
{
    public int BlogId { get; set; }

    public string TenantId { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public List<Post> Posts { get; } = new();
}
