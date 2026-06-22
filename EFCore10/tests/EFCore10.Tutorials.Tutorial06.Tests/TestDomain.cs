using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Tests;

internal static class TestDomain
{
    public static Author CreateAuthor() =>
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
            PasswordHash.HashPassword("Correct Horse Battery Staple 42!"));

    public static Blog CreateBlog(Author author) =>
        Blog.Create(
            BlogName.Create("EF Core Notes"),
            BlogUrl.Create("https://example.com/blog"),
            author);

    public static Post CreatePost() =>
        Post.Create(
            BlogId.NewId(),
            PostTitle.Create("Draft lifecycle"),
            PostContent.Create("Lifecycle content."));
}
