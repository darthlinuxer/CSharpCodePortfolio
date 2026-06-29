using System.Reflection;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Architecture tests + behavioural tests that prove the PhoneNumberValue
/// mirror has been retired in favour of an EF Core <c>ValueConverter</c>
/// that maps <c>Option&lt;PhoneNumber&gt;</c> directly to a nullable column.
/// </summary>
[TestClass]
public sealed class OptionPhoneNumberMappingTests
{
    /// <summary>
    /// Proves that the UserAccount aggregate does not carry an internal
    /// <c>PhoneNumberValue</c> mirror property. The domain exposes
    /// <c>Option&lt;PhoneNumber&gt;</c>; EF Core maps the abstraction
    /// directly via a value converter in the configuration class.
    /// </summary>
    [TestMethod]
    public void UserAccount_DoesNotExposePhoneNumberValueMirror()
    {
        var mirror = typeof(UserAccount).GetProperty(
            "PhoneNumberValue",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.IsNull(mirror,
            "UserAccount must not carry a PhoneNumberValue mirror property — Option<PhoneNumber> is the sole domain shape.");
    }

    /// <summary>
    /// Proves that an aggregate materialised by EF Core surfaces
    /// <c>None</c> when the persisted phone column is null and
    /// <c>Some(value)</c> when it is populated. This is the
    /// </summary>
    [TestMethod]
    public void EfCore_MaterializesAbsentPhoneAsOptionNone()
    {
        using var dbContext = CreateDbContext();
        var account = ValidAccount();
        account.ChangePhoneNumber(null);
        dbContext.Users.Add(account);
        dbContext.SaveChanges();

        using var verifyContext = CreateDbContext();
        var roundtrip = verifyContext.Users.Single();
        Assert.IsTrue(roundtrip.PhoneNumber.IsNone,
            "An aggregate persisted without a phone must materialise as Option<PhoneNumber>.None.");
    }

    /// <summary>
    /// Proves that an aggregate with a phone persists and materialises back
    /// with the same value, demonstrating the bidirectional Option<->string?
    /// value converter contract.
    /// </summary>
    [TestMethod]
    public void EfCore_MaterializesPresentPhoneAsOptionSome()
    {
        using var dbContext = CreateDbContext();
        var account = ValidAccount();
        account.ChangePhoneNumber("11999998888");
        dbContext.Users.Add(account);
        dbContext.SaveChanges();

        using var verifyContext = CreateDbContext();
        var roundtrip = verifyContext.Users.Single();
        var actualPhone = roundtrip.PhoneNumber.Match(Some: p => p.Value, None: () => string.Empty);

        Assert.AreEqual("11999998888", actualPhone);
    }

    private static UserAccount ValidAccount()
    {
        var account = UserAccount.Create(
            "Ada Lovelace",
            "ada@example.com",
            null,
            new FakeTimeProvider(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero)))
            .IfRight(a => a, _ => throw new AssertFailedException("Expected valid aggregate."));

        return account;
    }

    private static RegistrationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RegistrationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        var dbContext = new RegistrationDbContext(options);
        dbContext.Database.OpenConnection();
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}