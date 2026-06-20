using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial01;
public class CRUD
{
    public static async Task ExecuteAsync()
    {
        using var db = new BloggingContext();

        // Note: This sample requires the database to be created before running.
        Console.WriteLine($"Database path: {db.DbPath}.");

        // Create
        Console.WriteLine("Inserting a new blog");
        db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
        await db.SaveChangesAsync();

        // Read
        Console.WriteLine("Querying for a blog");
        var blog = await db.Blogs
            .OrderBy(b => b.BlogId)
            .FirstAsync();

        Console.WriteLine($"Blog uri retrieved: {blog.Url}");
        Console.WriteLine($"Number of Posts: {blog.Posts.Count()}");

        // Update
        Console.WriteLine("Updating the blog and adding a post...");
        blog.Url = "https://devblogs.microsoft.com/dotnet";
        blog.Posts.Add(
            new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
        await db.SaveChangesAsync();

        Console.WriteLine($"Blog uri retrieved: {blog.Url}");
        Console.WriteLine($"Number of Posts: {blog.Posts.Count()}");
        foreach(var post in blog.Posts)
            Console.WriteLine($"Post id: {post.BlogId} , Post content: {post.Content}");            

        // Delete
        Console.WriteLine("Delete the blog");
        db.Remove(blog);
        await db.SaveChangesAsync();

    }
}