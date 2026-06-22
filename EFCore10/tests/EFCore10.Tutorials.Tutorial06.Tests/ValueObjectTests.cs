using EFCore10.Tutorials.Tutorial06.Models;
using System.Reflection;

namespace EFCore10.Tutorials.Tutorial06.Tests;

[TestClass]
public sealed class ValueObjectTests
{
    [TestMethod]
    public void PersonValueObjectsRejectInvalidInput()
    {
        Assert.ThrowsExactly<DomainException>(() => PersonName.Create("Jo"));
        Assert.ThrowsExactly<DomainException>(() => Cpf.Create("123"));
        Assert.ThrowsExactly<DomainException>(() => Email.Create("invalid-email"));
        Assert.ThrowsExactly<DomainException>(() => PhoneNumber.Create("123"));
        Assert.ThrowsExactly<DomainException>(() => ZipCode.Create("123"));
        Assert.ThrowsExactly<DomainException>(() => StateCode.Create("Sao Paulo"));
        Assert.ThrowsExactly<DomainException>(() => UserName.Create("ab"));
        Assert.ThrowsExactly<DomainException>(() => PasswordHash.HashPassword(""));
        Assert.ThrowsExactly<DomainException>(() => PasswordHash.FromEncodedHash("not-a-hash"));
    }

    [TestMethod]
    public void PersonValueObjectsNormalizeValidInput()
    {
        Assert.AreEqual("Ada Lovelace", PersonName.Create("  Ada Lovelace  ").Value);
        Assert.AreEqual("12345678901", Cpf.Create("123.456.789-01").Value);
        Assert.AreEqual("ada@example.com", Email.Create("  ADA@Example.COM  ").Value);
        Assert.AreEqual("+5511999999999", PhoneNumber.Create("+55 (11) 99999-9999").Value);
        Assert.AreEqual("01000000", ZipCode.Create("01000-000").Value);
        Assert.AreEqual("SP", StateCode.Create(" sp ").Value);
        Assert.AreEqual("ada.lovelace", UserName.Create("  Ada.Lovelace  ").Value);
    }

    [TestMethod]
    public void BlogValueObjectsRejectInvalidInput()
    {
        Assert.ThrowsExactly<DomainException>(() => BlogName.Create("ab"));
        Assert.ThrowsExactly<DomainException>(() => BlogUrl.Create("ftp://example.com"));
        Assert.ThrowsExactly<DomainException>(() => PostTitle.Create("ab"));
        Assert.ThrowsExactly<DomainException>(() => PostContent.Create(""));
    }

    [TestMethod]
    public void BlogValueObjectsNormalizeValidInput()
    {
        Assert.AreEqual("EF Core Notes", BlogName.Create("  EF Core Notes  ").Value);
        Assert.AreEqual("https://example.com/blog", BlogUrl.Create("  https://example.com/blog  ").Value);
        Assert.AreEqual("DDD with EF Core", PostTitle.Create("  DDD with EF Core  ").Value);
        Assert.AreEqual("Rich model content.", PostContent.Create("  Rich model content.  ").Value);
    }

    [TestMethod]
    public void SingleValueObjectsDoNotExposePublicConstructors()
    {
        var valueObjectTypes = new[]
        {
            typeof(PersonName),
            typeof(Cpf),
            typeof(Email),
            typeof(PhoneNumber),
            typeof(ZipCode),
            typeof(StateCode),
            typeof(UserName),
            typeof(PasswordHash),
            typeof(BlogName),
            typeof(BlogUrl),
            typeof(PostTitle),
            typeof(PostContent)
        };

        Assert.IsTrue(valueObjectTypes.All(type => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length == 0));
        Assert.IsFalse(typeof(PersonName).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Any(method => method.Name == "Normalize"));
    }

    [TestMethod]
    public void PasswordHashUsesArgon2IdSaltAndConstantVerificationApi()
    {
        const string password = "Correct Horse Battery Staple 42!";

        var first = PasswordHash.HashPassword(password);
        var second = PasswordHash.HashPassword(password);

        StringAssert.StartsWith(first.Value, "$argon2id$v=19$m=19456,t=2,p=1$");
        Assert.AreNotEqual(password, first.Value);
        Assert.AreNotEqual(first.Value, second.Value);
        Assert.IsTrue(first.VerifyPassword(password));
        Assert.IsFalse(first.VerifyPassword("wrong password"));
        Assert.IsTrue(PasswordHash.FromEncodedHash(first.Value).VerifyPassword(password));
    }

    [TestMethod]
    public void PasswordHashRejectsUnsupportedArgon2Parameters()
    {
        var encodedHash = PasswordHash.HashPassword("Correct Horse Battery Staple 42!").Value;

        Assert.ThrowsExactly<DomainException>(() => PasswordHash.FromEncodedHash(encodedHash.Replace("m=19456", "m=1", StringComparison.Ordinal)));
        Assert.ThrowsExactly<DomainException>(() => PasswordHash.FromEncodedHash(encodedHash.Replace("t=2", "t=1", StringComparison.Ordinal)));
        Assert.ThrowsExactly<DomainException>(() => PasswordHash.FromEncodedHash(encodedHash.Replace("p=1", "p=2", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void StronglyTypedIdFactoriesUseUuidVersion7AndRejectEmptyGuid()
    {
        Assert.AreEqual(7, PersonId.NewId().Value.Version);
        Assert.AreEqual(7, BlogId.NewId().Value.Version);
        Assert.AreEqual(7, PostId.NewId().Value.Version);

        Assert.ThrowsExactly<DomainException>(() => PersonId.From(Guid.Empty));
        Assert.ThrowsExactly<DomainException>(() => AuthorId.From(Guid.Empty));
        Assert.ThrowsExactly<DomainException>(() => BlogId.From(Guid.Empty));
        Assert.ThrowsExactly<DomainException>(() => PostId.From(Guid.Empty));
    }
}
