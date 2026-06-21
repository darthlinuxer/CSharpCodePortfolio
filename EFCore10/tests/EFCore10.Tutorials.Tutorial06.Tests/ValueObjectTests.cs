using EFCore10.Tutorials.Tutorial06.Models;

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
        Assert.ThrowsExactly<DomainException>(() => PasswordHash.FromHash(new string('a', 501)));
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
}
