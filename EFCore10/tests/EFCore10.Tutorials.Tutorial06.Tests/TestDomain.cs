using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Tests;

internal static class TestDomain
{
    public static User CreateOwner() =>
        User.Register(
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

    public static User CreateAuthorUser() =>
        User.Register(
            PersonName.Create("Grace Hopper"),
            Cpf.Create("987.654.321-09"),
            Address.Create(
                "Rua B",
                "200",
                "Sao Paulo",
                StateCode.Create("SP"),
                ZipCode.Create("02000-000")),
            Contact.Create(
                Email.Create("grace@example.com"),
                PhoneNumber.Create("+55 (11) 98888-8888")),
            UserName.Create("grace.hopper"),
            PasswordHash.HashPassword("Amazing Grace 42!"));

    public static User CreateSuccessorOwner() =>
        User.Register(
            PersonName.Create("Mary Jackson"),
            Cpf.Create("111.222.333-44"),
            Address.Create(
                "Rua C",
                "300",
                "Rio de Janeiro",
                StateCode.Create("RJ"),
                ZipCode.Create("20000-000")),
            Contact.Create(
                Email.Create("mary@example.com"),
                PhoneNumber.Create("+55 (21) 97777-7777")),
            UserName.Create("mary.jackson"),
            PasswordHash.HashPassword("Hidden Figures 42!"));

    public static Blog CreateBlog(User owner) =>
        Blog.Create(
            BlogName.Create("EF Core Notes"),
            BlogUrl.Create("https://example.com/blog"),
            owner);

    public static Post CreatePost(UserId? postedByUserId = null) =>
        Post.Create(
            BlogId.NewId(),
            postedByUserId ?? UserId.NewId(),
            PostTitle.Create("Draft lifecycle"),
            PostContent.Create("Lifecycle content."));
}
