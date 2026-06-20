public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; } = string.Empty;

    public List<Post> Posts { get; } = new();
}
