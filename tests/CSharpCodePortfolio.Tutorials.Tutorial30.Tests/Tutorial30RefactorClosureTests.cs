using System.Reflection;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;
using LanguageExt;
using Microsoft.Extensions.Time.Testing;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Locks the final shape expected by the Tutorial30 monadic DDD refactor.
/// </summary>
[TestClass]
public sealed class Tutorial30RefactorClosureTests
{
    [TestMethod]
    public void Tutorial30Project_PinsCSharp14()
    {
        var projectFile = Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30", "CSharpCodePortfolio.Tutorials.Tutorial30.csproj");

        var projectXml = File.ReadAllText(projectFile);

        StringAssert.Contains(projectXml, "<LangVersion>14.0</LangVersion>");
    }

    [TestMethod]
    public void PublicContracts_RemoveDocumentFromRegistrationFlow()
    {
        var requestProperties = typeof(RegisterUserRequest).GetProperties().Select(property => property.Name).ToArray();
        var commandDtoProperties = typeof(RegisteredUserDto).GetProperties().Select(property => property.Name).ToArray();
        var queryDtoProperties = typeof(UserAccountQueryDto).GetProperties().Select(property => property.Name).ToArray();

        CollectionAssert.DoesNotContain(requestProperties, "Document");
        CollectionAssert.DoesNotContain(commandDtoProperties, "Document");
        CollectionAssert.DoesNotContain(queryDtoProperties, "Document");
        Assert.IsNull(typeof(UserAccount).GetProperty("Document", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        Assert.IsNull(typeof(UserAccount).GetMethod("NormalizeDocument", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
        Assert.IsNull(typeof(IUserAccountLookup).GetMethod("DocumentExistsAsync"));
    }

    [TestMethod]
    public void DomainErrors_ExposeCategoryTaxonomy()
    {
        var categoryProperty = typeof(DomainError).GetProperty("Category");
        var categoryType = typeof(DomainError).Assembly.GetType("CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors.DomainErrorCategory");

        Assert.IsNotNull(categoryProperty);
        Assert.IsNotNull(categoryType);
        Assert.AreEqual(categoryType, categoryProperty.PropertyType);
        Assert.AreEqual("validation", new EmailInvalidError().Category.Value);
        Assert.AreEqual("conflict", new UserAccountEmailDuplicateError().Category.Value);
    }

    [TestMethod]
    public void AbstractEntity_UsesTimestampAuditWithoutActorCoupling()
    {
        Assert.AreEqual("AbstractEntity`1", typeof(UserAccount).BaseType?.GetGenericTypeDefinition().Name);
        Assert.IsNull(typeof(UserAccount).GetProperty("CreatedBy"));
        Assert.IsNull(typeof(UserAccount).GetProperty("LastModifiedBy"));
    }

    [TestMethod]
    public void TimestampUtcNow_UsesInjectedClock()
    {
        var now = new DateTimeOffset(2026, 6, 29, 12, 30, 0, TimeSpan.Zero);
        var clock = new FakeTimeProvider(now);
        var method = typeof(Timestamp).GetMethod(nameof(Timestamp.UtcNow), BindingFlags.Static | BindingFlags.Public, [typeof(TimeProvider)]);

        Assert.IsNotNull(method);
        var timestamp = (Timestamp)method.Invoke(null, [clock])!;

        Assert.AreEqual(now.UtcDateTime, timestamp.Value);
    }

    [TestMethod]
    public void UserAccountCreate_InvalidInputStaysLeft()
    {
        var result = UserAccount.Create(" ", null, "1", new FakeTimeProvider());

        Assert.IsTrue(result.IsLeft);
    }

    [TestMethod]
    public void UserAccount_NoOpMutationsDoNotRaiseDomainEvents()
    {
        var account = GetRight(UserAccount.Create("Ada Lovelace", "ada@example.com", "(11) 99999-8888", new FakeTimeProvider()));
        account.ClearDomainEvents();
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 6, 29, 13, 0, 0, TimeSpan.Zero));

        account.Rename("Ada Lovelace", clock);
        account.ChangeEmail("ada@example.com", clock);
        account.ChangePhoneNumber("(11) 99999-8888", clock);

        Assert.IsEmpty(account.DomainEvents.ToArray());
        Assert.IsTrue(account.LastModified.IsNone);
    }

    [TestMethod]
    public void RegisterUserService_DependsOnPortsAndInjectedClock()
    {
        var constructorParameterTypes = typeof(RegisterUserService)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { typeof(IUserAccountLookup), typeof(IUserAccountWriter), typeof(IRegistrationUnitOfWork), typeof(TimeProvider) },
            constructorParameterTypes);
    }

    private static TRight GetRight<TLeft, TRight>(Either<TLeft, TRight> result) =>
        result.Match(
            Right: value => value,
            Left: error => throw new AssertFailedException($"Expected Right, got Left({error})."));

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "CSharpCodePortfolio.slnx");
            if (File.Exists(candidate))
                return current.FullName;

            current = current.Parent;
        }

        throw new AssertFailedException("Could not find repository root.");
    }
}
