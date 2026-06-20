using Microsoft.EntityFrameworkCore;
using EFCore10.Tutorials.Tutorial02.Context;
using EFCore10.Tutorials.Tutorial02.Models;

namespace EFCore10.Tutorials.Tutorial02;

public sealed class CRUD(BloggingContext db)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        // Create
        Console.WriteLine("Inserting a new blog");
        db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Read
        Console.WriteLine("Querying for a blog");
        var blog = await db.Blogs
            .Include(blog => blog.Posts)
            .OrderBy(b => b.BlogId)
            .FirstAsync(cancellationToken).ConfigureAwait(false);

        Console.WriteLine($"Blog uri retrieved: {blog.Url}");
        Console.WriteLine($"Number of Posts: {blog.Posts.Count}");

        // Update
        Console.WriteLine("Updating the blog and adding a post...");
        blog.Url = "https://devblogs.microsoft.com/dotnet";
        blog.Posts.Add(
            new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Console.WriteLine($"Blog uri retrieved: {blog.Url}");
        Console.WriteLine($"Number of Posts: {blog.Posts.Count}");
        foreach (var post in blog.Posts)
        {
            Console.WriteLine($"Post id: {post.BlogId}, Post content: {post.Content}");
        }

        // Delete
        Console.WriteLine("Delete the blog");
        db.Remove(blog);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
