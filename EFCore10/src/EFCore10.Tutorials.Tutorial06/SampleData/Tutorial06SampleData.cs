using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.SampleData;

internal static class Tutorial06SampleData
{
    public static Author CreateDemoAuthor() =>
        Author.Create(
            PersonName.Create("Ada Lovelace"),
            Cpf.Create("123.456.789-01"),
            Address.Create(
                "Rua A",
                "100",
                "Sao Paulo",
                StateCode.Create("SP"),
                ZipCode.Create("01000-000")),
            Contact.Create(
                Email.Create("ada@example.com"),
                PhoneNumber.Create("+55 (11) 99999-9999")),
            UserName.Create("ada.lovelace"),
            PasswordHash.FromHash("hashed-password"));

    public static Blog CreateDemoBlog(Author author) =>
        Blog.Create(
            BlogName.Create("EF Core Notes"),
            BlogUrl.Create("https://example.com/blog"),
            author);

    public static Post AddArchivedPost(Blog blog)
    {
        var post = blog.AddPost(
            PostTitle.Create("DDD with EF Core 10"),
            PostContent.Create("Value objects, TPH inheritance, behavioral state, and in-memory domain events."));

        post.Publish();
        post.Archive();

        return post;
    }
}
