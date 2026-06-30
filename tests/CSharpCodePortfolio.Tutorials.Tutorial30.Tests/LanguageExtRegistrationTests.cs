using System.Reflection;
using CSharpCodePortfolio.Tutorials.Tutorial30;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Verifies the functional registration tutorial behavior and its architectural boundaries.
/// </summary>
[TestClass]
public sealed class LanguageExtRegistrationTests
{
    [TestMethod]
    public void RequiredValueObjects_ReturnErrorsWithoutBusinessExceptions()
    {
        Assert.IsInstanceOfType<PersonNameRequiredError>(GetOnlyError(PersonName.Create(Some(" "))));
        Assert.IsInstanceOfType<EmailInvalidError>(GetOnlyError(Email.Create(None)));
        Assert.IsInstanceOfType<PhoneNumberInvalidError>(GetOnlyError(PhoneNumber.CreateOptional(Some("1"))));
    }

    [TestMethod]
    public void UserAccountCreate_GeneratesIdentityTimestampAndRegisteredEvent()
    {
        var now = new DateTimeOffset(2026, 6, 29, 12, 0, 0, TimeSpan.Zero);
        var account = CreateValidAccount(new FakeTimeProvider(now));

        Assert.AreNotEqual(Guid.Empty, account.Id);
        Assert.AreEqual(7, account.Id.Version);
        Assert.AreEqual(now.UtcDateTime, account.CreatedAt.Match(Some: timestamp => timestamp.Value, None: () => default));

        var domainEvent = Assert.IsInstanceOfType<UserAccountRegisteredDomainEvent>(account.DomainEvents.Single());
        Assert.AreEqual(account.Id, domainEvent.UserId);
        Assert.AreEqual(account.Email, domainEvent.Email);
        Assert.AreEqual(now.UtcDateTime, domainEvent.OccurredAtUtc.Value);
        Assert.AreEqual("UserAccount", domainEvent.AggregateName);
    }

    [TestMethod]
    public void UserAccountCreate_AccumulatesRequiredFieldErrors()
    {
        var result = UserAccount.Create(Some(" "), None, Some("1"), TimeProvider.System);

        var errors = GetLeft(result).Select(error => error.GetType()).ToArray();

        CollectionAssert.Contains(errors, typeof(PersonNameRequiredError));
        CollectionAssert.Contains(errors, typeof(EmailInvalidError));
        CollectionAssert.Contains(errors, typeof(PhoneNumberInvalidError));
    }

    [TestMethod]
    public void UserAccountChangeMethods_RaiseTypedDomainEventsForRealChanges()
    {
        var now = new DateTimeOffset(2026, 6, 29, 12, 0, 0, TimeSpan.Zero);
        var clock = new FakeTimeProvider(now);
        var account = CreateValidAccount(clock);
        account.ClearDomainEvents();

        Assert.IsTrue(account.Rename(Some("Augusta Ada"), clock).IsRight);
        Assert.IsTrue(account.ChangeEmail(Some("ada.lovelace@example.com"), clock).IsRight);
        Assert.IsTrue(account.ChangePhoneNumber(Some("(11) 99999-8888"), clock).IsRight);

        var events = account.DomainEvents.ToArray();
        Assert.HasCount(3, events);

        var nameChanged = Assert.IsInstanceOfType<UserAccountNameChangedDomainEvent>(events[0]);
        Assert.AreEqual(account.Id, nameChanged.UserId);
        Assert.AreEqual("Ada Lovelace", nameChanged.PreviousName.Value);
        Assert.AreEqual("Augusta Ada", nameChanged.NewName.Value);
        Assert.AreEqual(now.UtcDateTime, nameChanged.OccurredAtUtc.Value);

        var emailChanged = Assert.IsInstanceOfType<UserAccountEmailChangedDomainEvent>(events[1]);
        Assert.AreEqual(account.Id, emailChanged.UserId);
        Assert.AreEqual("ada@example.com", emailChanged.PreviousEmail.Value);
        Assert.AreEqual("ada.lovelace@example.com", emailChanged.NewEmail.Value);
        Assert.AreEqual(now.UtcDateTime, emailChanged.OccurredAtUtc.Value);

        var phoneChanged = Assert.IsInstanceOfType<UserAccountPhoneNumberChangedDomainEvent>(events[2]);
        Assert.AreEqual(account.Id, phoneChanged.UserId);
        Assert.IsTrue(phoneChanged.PreviousPhoneNumber.IsNone);
        Assert.AreEqual("11999998888", phoneChanged.NewPhoneNumber.Match(Some: phone => phone.Value, None: () => string.Empty));
        Assert.AreEqual(now.UtcDateTime, phoneChanged.OccurredAtUtc.Value);
        Assert.IsTrue(account.LastModified.IsSome);
    }

    [TestMethod]
    public void UserAccountNoOpChanges_DoNotMarkModifiedOrRaiseEvents()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 6, 29, 12, 0, 0, TimeSpan.Zero));
        var account = CreateValidAccount(clock);
        account.ClearDomainEvents();

        Assert.IsTrue(account.Rename(Some("Ada Lovelace"), clock).IsRight);
        Assert.IsTrue(account.ChangeEmail(Some("ada@example.com"), clock).IsRight);
        Assert.IsTrue(account.ChangePhoneNumber(None, clock).IsRight);

        Assert.IsEmpty(account.DomainEvents.ToArray());
        Assert.IsTrue(account.LastModified.IsNone);
    }

    [TestMethod]
    public void TimestampCreate_RejectsNonUtcDateTimeWithoutException()
    {
        var localTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Local);

        var result = Timestamp.Create(localTime);

        Assert.IsInstanceOfType<TimestampUtcRequiredError>(GetLeft(result));
    }

    [TestMethod]
    public void RegistrationDbContext_MapsUserAccountWithoutDocumentAndWithUniqueEmail()
    {
        using var dbContext = CreateDbContext();

        var userEntity = dbContext.Model.FindEntityType(typeof(UserAccount));

        Assert.IsNotNull(userEntity);
        Assert.IsNull(userEntity.FindProperty("Document"));
        Assert.IsNotNull(userEntity.FindProperty(nameof(UserAccount.Name)));
        Assert.IsNotNull(userEntity.FindProperty(nameof(UserAccount.Email)));
        Assert.IsNull(userEntity.FindProperty(nameof(UserAccount.PhoneNumber)));
        Assert.IsNotNull(userEntity.FindProperty("PhoneNumberValue"));
        Assert.IsTrue(userEntity.GetIndexes()
            .Where(index => index.IsUnique)
            .Any(index => index.Properties.Select(property => property.GetColumnName()).SequenceEqual(["Email"])));
    }

    [TestMethod]
    public void Tutorial30_PhysicalFoldersShowCleanArchitectureRings()
    {
        var tutorialRoot = FindTutorialRoot();

        Assert.IsTrue(Directory.Exists(Path.Combine(tutorialRoot, "01-Domain", "Common")));
        Assert.IsTrue(Directory.Exists(Path.Combine(tutorialRoot, "01-Domain", "Aggregates", "UserAccounts")));
        Assert.IsTrue(Directory.Exists(Path.Combine(tutorialRoot, "02-Application")));
        Assert.IsTrue(Directory.Exists(Path.Combine(tutorialRoot, "03-Infrastructure")));
        Assert.IsTrue(Directory.Exists(Path.Combine(tutorialRoot, "03-Presentation")));
    }

    [TestMethod]
    public void DomainAndApplicationLayers_DoNotReferenceOuterAdapters()
    {
        var forbiddenReferences = typeof(UserAccount).Assembly
            .GetTypes()
            .Where(type => type.Namespace?.Contains(".Domain", StringComparison.Ordinal) == true
                || type.Namespace?.Contains(".Application", StringComparison.Ordinal) == true)
            .SelectMany(type => type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            .Select(member => member.ToString() ?? string.Empty)
            .Where(signature =>
                signature.Contains("DbContext", StringComparison.Ordinal) ||
                signature.Contains("Infrastructure", StringComparison.Ordinal) ||
                signature.Contains("AspNetCore", StringComparison.Ordinal))
            .ToArray();

        Assert.IsEmpty(forbiddenReferences);
    }

    [TestMethod]
    public void AbstractEntity_ExposesOnlyTimestampAuditAndDomainEvents()
    {
        Assert.AreEqual(typeof(AbstractEntity<UserAccount, Guid>), typeof(UserAccount).BaseType);
        Assert.IsNull(typeof(UserAccount).GetProperty("CreatedBy"));
        Assert.IsNull(typeof(UserAccount).GetProperty("LastModifiedBy"));
        Assert.IsEmpty(typeof(AbstractEntity<UserAccount, Guid>)
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(property => property.Name.EndsWith("Value", StringComparison.Ordinal))
            .ToArray());
    }

    [TestMethod]
    public void UserAccountDomainEvents_AreScopedToUserAccountAggregate()
    {
        var domainEvent = new UserAccountRegisteredDomainEvent(
            Guid.NewGuid(),
            GetRight(Email.Create(Some("ada@example.com"))),
            Timestamp.UtcNow(TimeProvider.System));

        Assert.IsInstanceOfType<AbstractDomainEvent<UserAccount>>(domainEvent);
        Assert.AreEqual(typeof(Seq<AbstractDomainEvent<UserAccount>>), typeof(UserAccount).GetProperty(nameof(UserAccount.DomainEvents))?.PropertyType);
        Assert.AreEqual("UserAccount", domainEvent.AggregateName);
        StringAssert.StartsWith(domainEvent.ToString(), "UserAccount:");
        StringAssert.StartsWith(CreateValidAccount().ToString(), "UserAccount(");
    }

    [TestMethod]
    public async Task RegisterAsync_RejectsMissingEmail()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.RegisterAsync(new RegisterUserRequest("Ada Lovelace", null, null));

        Assert.IsInstanceOfType<EmailInvalidError>(GetOnlyError(result));
        Assert.AreEqual(0, await dbContext.Users.CountAsync());
    }

    [TestMethod]
    public async Task RegisterAsync_CreatesUserWithEmailAndPhone()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.RegisterAsync(new RegisterUserRequest("Grace Hopper", "GRACE@EXAMPLE.COM", "(11) 99999-8888"));

        var user = GetRight(result);
        Assert.AreEqual("grace@example.com", user.Email);
        Assert.AreEqual("11999998888", user.PhoneNumber.Match(Some: phone => phone, None: () => string.Empty));
    }

    [TestMethod]
    public async Task RegistrationDbContextCommitAsync_ClearsDomainEventsAfterCommit()
    {
        await using var dbContext = CreateDbContext();
        var writer = new EfUserAccountWriter(dbContext);
        var account = CreateValidAccount();

        writer.Add(account);
        var commit = await ((IRegistrationUnitOfWork)dbContext).CommitAsync(CancellationToken.None);

        Assert.IsTrue(commit.IsRight);
        Assert.IsEmpty(account.DomainEvents.ToArray());
        Assert.AreEqual(1, await dbContext.Users.CountAsync());
    }

    [TestMethod]
    public async Task RegisterAsync_ReturnsConflictErrorWhenUniqueConstraintRejectsDuplicateEmail()
    {
        await using var dbContext = CreateDbContext();
        var writer = new EfUserAccountWriter(dbContext);
        var existing = CreateValidAccount();

        writer.Add(existing);
        Assert.IsTrue((await ((IRegistrationUnitOfWork)dbContext).CommitAsync(CancellationToken.None)).IsRight);

        var service = new RegisterUserService(
            new AlwaysEmptyUserAccountLookup(),
            writer,
            dbContext,
            TimeProvider.System);

        var result = await service.RegisterAsync(new RegisterUserRequest("Outra Ada", "ADA@EXAMPLE.COM", null));

        Assert.IsInstanceOfType<UserAccountEmailDuplicateError>(GetOnlyError(result));
        Assert.IsFalse(dbContext.ChangeTracker.Entries<UserAccount>().Any(entry => entry.State == EntityState.Added));
    }

    [TestMethod]
    public async Task RegisterAsync_ReturnsConflictErrorForDuplicateEmail()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        await service.RegisterAsync(new RegisterUserRequest("Com Email", "same@example.com", null));

        var duplicateEmail = await service.RegisterAsync(new RegisterUserRequest("Outro Email", "same@example.com", null));

        Assert.IsInstanceOfType<UserAccountEmailDuplicateError>(GetOnlyError(duplicateEmail));
    }

    [TestMethod]
    public void RegistrationEndpoint_MapsApplicationResultToHttpStatus()
    {
        var created = RegistrationEndpoint.ToHttpResult(
            Right<Seq<DomainError>, RegisteredUserDto>(
                new RegisteredUserDto(Guid.NewGuid(), "Ada", "ada@example.com", None)));
        var badRequest = RegistrationEndpoint.ToHttpResult(
            Left<Seq<DomainError>, RegisteredUserDto>(
                Seq1<DomainError>(new EmailInvalidError())));
        var conflict = RegistrationEndpoint.ToHttpResult(
            Left<Seq<DomainError>, RegisteredUserDto>(
                Seq1<DomainError>(new UserAccountEmailDuplicateError())));

        Assert.AreEqual(StatusCodes.Status201Created, GetStatus(created));
        Assert.AreEqual(StatusCodes.Status400BadRequest, GetStatus(badRequest));
        Assert.AreEqual(StatusCodes.Status409Conflict, GetStatus(conflict));
    }

    [TestMethod]
    public void ProblemResult_ReturnsProblemDetailsWithErrorsExtension()
    {
        var result = ProblemResult.FromErrors(Seq1<DomainError>(new EmailInvalidError()));

        var problem = Assert.IsInstanceOfType<ProblemHttpResult>(result);

        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.IsNotNull(problem.ProblemDetails);
        Assert.IsTrue(problem.ProblemDetails.Extensions.ContainsKey("errors"));
    }

    [TestMethod]
    public void LanguageExtConceptsDemo_ProvesCoreTypes()
    {
        var report = LanguageExtConceptsDemo.Run();

        Assert.IsTrue(report.AbsentEmailIsNone);
        Assert.AreEqual("ada@example.com", report.NormalizedEmail);
        Assert.AreEqual(15, report.EmailLength);
        Assert.AreEqual("persistência representada como sucesso", report.FinMessage);
        Assert.AreEqual(123, report.TryValue);
        Assert.AreEqual(12, report.SeqTotal);
        Assert.AreEqual(11, report.ComposedLength);
    }

    [TestMethod]
    public void LanguageExtCoreTutorial_PrintsDomainEventToStringEvidence()
    {
        var method = typeof(LanguageExtCoreTutorial).GetMethod(
            "CreateDomainEventEvidence",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.IsNotNull(method);
        var evidence = (string)method.Invoke(null, [])!;

        StringAssert.Contains(evidence, "UserAccount: UserAccountRegisteredDomainEvent");
        StringAssert.Contains(evidence, "UserAccount: UserAccountNameChangedDomainEvent");
        StringAssert.Contains(evidence, "UserAccount: UserAccountEmailChangedDomainEvent");
        StringAssert.Contains(evidence, "UserAccount: UserAccountPhoneNumberChangedDomainEvent");
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

    private static RegisterUserService CreateService(RegistrationDbContext dbContext) =>
        new(
            new EfUserAccountLookup(dbContext),
            new EfUserAccountWriter(dbContext),
            dbContext,
            TimeProvider.System);

    private static TRight GetRight<TLeft, TRight>(Either<TLeft, TRight> result) =>
        result.Match(
            Right: value => value,
            Left: error => throw new AssertFailedException($"Expected Right, got Left({error})."));

    private static TLeft GetLeft<TLeft, TRight>(Either<TLeft, TRight> result) =>
        result.Match(
            Right: value => throw new AssertFailedException($"Expected Left, got Right({value})."),
            Left: error => error);

    private static DomainError GetOnlyError<T>(Either<Seq<DomainError>, T> result)
    {
        var errors = GetLeft(result).ToArray();

        Assert.HasCount(1, errors);
        return errors[0];
    }

    private static DomainError GetOnlyError<T>(Either<DomainError, T> result) =>
        GetLeft(result);

    private static UserAccount CreateValidAccount(TimeProvider? clock = null) =>
        GetRight(UserAccount.Create(Some("Ada Lovelace"), Some("ada@example.com"), None, clock ?? TimeProvider.System));

    private static string FindTutorialRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "src", "CSharpCodePortfolio.Tutorials.Tutorial30");
            if (Directory.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new AssertFailedException("Could not find Tutorial30 source folder.");
    }

    private static int GetStatus(IResult result) =>
        result is IStatusCodeHttpResult statusCodeResult
            ? statusCodeResult.StatusCode ?? StatusCodes.Status200OK
            : StatusCodes.Status200OK;

    private sealed class AlwaysEmptyUserAccountLookup : IUserAccountLookup
    {
        public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task<Option<UserAccountQueryDto>> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<Option<UserAccountQueryDto>>(None);
    }
}
