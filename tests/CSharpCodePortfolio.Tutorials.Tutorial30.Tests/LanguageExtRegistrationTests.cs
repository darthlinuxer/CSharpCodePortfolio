using CSharpCodePortfolio.Tutorials.Tutorial30;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Verifies the functional registration tutorial behavior and its HTTP mapping.
/// </summary>
[TestClass]
public sealed class LanguageExtRegistrationTests
{
    /// <summary>
    /// Proves that missing or malformed email is invalid because email is required.
    /// </summary>
    [TestMethod]
    public void EmailRequired_RejectsMissingAndInvalidEmail()
    {
        var absent = Email.Create(null);
        var invalid = Email.Create("email-invalido");

        Assert.IsInstanceOfType<EmailInvalidError>(GetLeft(absent));
        Assert.IsInstanceOfType<EmailInvalidError>(GetLeft(invalid));
    }

    /// <summary>
    /// Proves that required value objects return errors instead of throwing business exceptions.
    /// </summary>
    [TestMethod]
    public void RequiredValueObjects_ReturnErrorsWithoutBusinessExceptions()
    {
        var name = PersonName.Create(" ");
        var document = UserAccount.NormalizeDocument(null);

        Assert.IsInstanceOfType<PersonNameRequiredError>(GetLeft(name));
        Assert.IsInstanceOfType<UserAccountDocumentInvalidError>(GetLeft(document));
    }

    /// <summary>
    /// Proves that the aggregate creates a domain identity automatically.
    /// </summary>
    [TestMethod]
    public void UserAccountCreate_GeneratesIdentity()
    {
        var account = CreateValidAccount();

        Assert.AreNotEqual(Guid.Empty, account.Id);
        Assert.AreEqual(7, account.Id.Version);
    }

    /// <summary>
    /// Proves that aggregate behavior records a domain event when registration succeeds.
    /// </summary>
    [TestMethod]
    public void UserAccountCreate_RaisesUserAccountRegisteredDomainEvent()
    {
        var account = GetRight(UserAccount.Create("Ada Lovelace", "DOC-10000", "ada@example.com", null));

        var events = account.DomainEvents.ToArray();

        Assert.HasCount(1, events);
        var domainEvent = Assert.IsInstanceOfType<UserAccountRegisteredDomainEvent>(events[0]);
        Assert.AreEqual(account.Id, domainEvent.UserId);
        Assert.AreEqual(account.Document, domainEvent.Document);
        Assert.AreEqual(account.Email, domainEvent.Email);
        Assert.AreEqual(UserAccountDomainEventTypes.Registered, domainEvent.EventType);
    }

    /// <summary>
    /// Proves that required aggregate creation errors are accumulated by the domain result.
    /// </summary>
    [TestMethod]
    public void UserAccountCreate_AccumulatesRequiredFieldErrors()
    {
        var result = UserAccount.Create(" ", " ", null, "1");

        var errors = GetLeft(result).Select(error => error.GetType()).ToArray();

        CollectionAssert.Contains(errors, typeof(PersonNameRequiredError));
        CollectionAssert.Contains(errors, typeof(UserAccountDocumentInvalidError));
        CollectionAssert.Contains(errors, typeof(EmailInvalidError));
        CollectionAssert.Contains(errors, typeof(PhoneNumberInvalidError));
    }

    /// <summary>
    /// Proves that aggregate behavior raises typed domain events for state changes.
    /// </summary>
    [TestMethod]
    public void UserAccountChangeMethods_RaiseTypedDomainEvents()
    {
        var account = GetRight(UserAccount.Create("Ada Lovelace", "DOC-10000", "ada@example.com", null));
        account.ClearDomainEvents();

        Assert.IsTrue(account.Rename("Augusta Ada").IsRight);
        Assert.IsTrue(account.ChangeEmail("ada.lovelace@example.com").IsRight);
        Assert.IsTrue(account.ChangePhoneNumber("(11) 99999-8888").IsRight);

        var eventTypes = account.DomainEvents.Map(domainEvent => domainEvent.EventType).ToArray();
        CollectionAssert.Contains(eventTypes, UserAccountDomainEventTypes.NameChanged);
        CollectionAssert.Contains(eventTypes, UserAccountDomainEventTypes.EmailChanged);
        CollectionAssert.Contains(eventTypes, UserAccountDomainEventTypes.PhoneNumberChanged);
    }

    /// <summary>
    /// Proves that non-UTC DateTime values become domain errors instead of exceptions.
    /// </summary>
    [TestMethod]
    public void TimestampCreate_RejectsNonUtcDateTimeWithoutException()
    {
        var localTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Local);

        var result = Timestamp.Create(localTime);

        Assert.IsInstanceOfType<TimestampUtcRequiredError>(GetLeft(result));
    }

    /// <summary>
    /// Proves that EF Core maps the aggregate directly and uses complex properties for value objects.
    /// </summary>
    [TestMethod]
    public void RegistrationDbContext_MapsUserAccountWithComplexProperties()
    {
        using var dbContext = CreateDbContext();

        var userEntity = dbContext.Model.FindEntityType(typeof(UserAccount));

        Assert.IsNotNull(userEntity);
        var complexPropertyNames = userEntity.GetComplexProperties().Select(property => property.Name).ToArray();
        Assert.Contains(nameof(UserAccount.Name), complexPropertyNames);
        Assert.Contains(nameof(UserAccount.Email), complexPropertyNames);
        Assert.Contains("PhoneNumberValue", complexPropertyNames);
        Assert.IsFalse(complexPropertyNames.Any(name => name.Contains("ForPersistence", StringComparison.Ordinal)));
        Assert.IsNotNull(userEntity.FindProperty(nameof(UserAccount.Document)));
    }

    /// <summary>
    /// Proves that the domain layer has no dependency on infrastructure or EF Core.
    /// </summary>
    [TestMethod]
    public void DomainLayer_DoesNotReferenceInfrastructureOrEntityFramework()
    {
        var domainTypes = typeof(UserAccount).Assembly
            .GetTypes()
            .Where(type => type.Namespace?.Contains(".Domain", StringComparison.Ordinal) == true)
            .ToArray();

        var forbiddenReferences = domainTypes
            .SelectMany(type => type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            .Select(member => member.ToString() ?? string.Empty)
            .Where(signature =>
                signature.Contains("DbContext", StringComparison.Ordinal) ||
                signature.Contains("Infrastructure", StringComparison.Ordinal))
            .ToArray();

        Assert.IsEmpty(forbiddenReferences);
    }

    /// <summary>
    /// Proves that the entity base does not expose duplicated nullable properties only for EF mapping.
    /// </summary>
    [TestMethod]
    public void AbstractEntity_DoesNotExposeMappingOnlyAuditProperties()
    {
        var mappingOnlyProperties = typeof(AbstractEntity<Guid>)
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(property => property.Name.EndsWith("Value", StringComparison.Ordinal))
            .Select(property => property.Name)
            .ToArray();

        Assert.IsEmpty(mappingOnlyProperties);
    }

    /// <summary>
    /// Proves that the old global string-based error catalog is gone.
    /// </summary>
    [TestMethod]
    public void DomainLayer_DoesNotExposeGlobalDomainErrorsCatalog()
    {
        var domainErrorsType = typeof(UserAccount).Assembly.GetType(
            "CSharpCodePortfolio.Tutorials.Tutorial30.Domain.DomainErrors");

        Assert.IsNull(domainErrorsType);
    }

    /// <summary>
    /// Proves that application code has no dependency on EF Core, Infrastructure, or ASP.NET Core.
    /// </summary>
    [TestMethod]
    public void ApplicationLayer_DoesNotReferenceInfrastructureEntityFrameworkOrAspNetCore()
    {
        var applicationTypes = typeof(RegisterUserService).Assembly
            .GetTypes()
            .Where(type => type.Namespace?.Contains(".Application", StringComparison.Ordinal) == true)
            .ToArray();

        var forbiddenReferences = applicationTypes
            .SelectMany(type => type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            .Select(member => member.ToString() ?? string.Empty)
            .Where(signature =>
                signature.Contains("Infrastructure", StringComparison.Ordinal) ||
                signature.Contains("EntityFrameworkCore", StringComparison.Ordinal) ||
                signature.Contains("AspNetCore", StringComparison.Ordinal))
            .ToArray();

        Assert.IsEmpty(forbiddenReferences);
    }

    /// <summary>
    /// Proves that the application service depends on application ports, not EF Core infrastructure types.
    /// </summary>
    [TestMethod]
    public void RegisterUserService_DependsOnApplicationPortsOnly()
    {
        var constructorParameterTypes = typeof(RegisterUserService)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { typeof(IUserAccountLookup), typeof(IUserAccountWriter), typeof(IRegistrationUnitOfWork) },
            constructorParameterTypes);
    }

    /// <summary>
    /// Proves that HTTP adapters live at the presentation boundary.
    /// </summary>
    [TestMethod]
    public void RegistrationEndpoint_LivesInPresentationHttpNamespace()
    {
        Assert.AreEqual(
            "CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http",
            typeof(RegistrationEndpoint).Namespace);
    }

    /// <summary>
    /// Proves that registration fails when required email is absent.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_RejectsMissingEmail()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.RegisterAsync(
            new RegisterUserRequest("Ada Lovelace", "DOC-10000", null, null));

        Assert.IsInstanceOfType<EmailInvalidError>(GetOnlyError(result));
        Assert.AreEqual(0, await dbContext.Users.CountAsync());
    }

    /// <summary>
    /// Proves that supplied required email and optional phone are normalized and persisted.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_CreatesUserWithEmailAndPhone()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.RegisterAsync(
            new RegisterUserRequest("Grace Hopper", "DOC-20000", "GRACE@EXAMPLE.COM", "(11) 99999-8888"));

        var user = GetRight(result);
        Assert.AreEqual("grace@example.com", user.Email);
        Assert.AreEqual("11999998888", user.PhoneNumber.Match(Some: phone => phone, None: () => string.Empty));
    }

    /// <summary>
    /// Proves that the persistence boundary clears captured domain events after commit.
    /// </summary>
    [TestMethod]
    public void EfUserAccountWriterAdd_DoesNotCommitOrClearDomainEvents()
    {
        using var dbContext = CreateDbContext();
        var writer = new EfUserAccountWriter(dbContext);
        var account = CreateValidAccount();

        Assert.HasCount(1, account.DomainEvents.ToArray());

        writer.Add(account);

        Assert.HasCount(1, account.DomainEvents.ToArray());
        Assert.AreEqual(EntityState.Added, dbContext.Entry(account).State);
    }

    /// <summary>
    /// Proves that the EF Core Unit of Work commits and clears domain events after persistence succeeds.
    /// </summary>
    [TestMethod]
    public async Task RegistrationDbContextSaveChangesAsync_ClearsDomainEventsAfterCommit()
    {
        await using var dbContext = CreateDbContext();
        var writer = new EfUserAccountWriter(dbContext);
        var account = CreateValidAccount();

        writer.Add(account);
        await ((IRegistrationUnitOfWork)dbContext).SaveChangesAsync(CancellationToken.None);

        Assert.IsEmpty(account.DomainEvents.ToArray());
        Assert.AreEqual(1, await dbContext.Users.CountAsync());
    }

    /// <summary>
    /// Proves that registration uniqueness is decided by the aggregate from application-provided facts.
    /// </summary>
    [TestMethod]
    public void UserAccountEnsureCanBeRegistered_ReturnsTypedDuplicateErrors()
    {
        var account = CreateValidAccount();

        var duplicateDocument = account.EnsureCanBeRegistered(documentExists: true, emailExists: false);
        var duplicateEmail = account.EnsureCanBeRegistered(documentExists: false, emailExists: true);

        Assert.IsInstanceOfType<UserAccountDocumentDuplicateError>(GetOnlyError(duplicateDocument));
        Assert.IsInstanceOfType<UserAccountEmailDuplicateError>(GetOnlyError(duplicateEmail));
    }

    /// <summary>
    /// Proves that duplicate document is an expected conflict error.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_ReturnsConflictErrorForDuplicateDocument()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        await service.RegisterAsync(new RegisterUserRequest("Ada Lovelace", "DOC-10000", "ada@example.com", null));

        var result = await service.RegisterAsync(
            new RegisterUserRequest("Outra Ada", "DOC-10000", "outra@example.com", null));

        Assert.IsInstanceOfType<UserAccountDocumentDuplicateError>(GetOnlyError(result));
    }

    /// <summary>
    /// Proves that required email uniqueness is an expected conflict error.
    /// </summary>
    [TestMethod]
    public async Task RegisterAsync_ReturnsConflictErrorForDuplicateEmail()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        await service.RegisterAsync(new RegisterUserRequest("Com Email", "DOC-20000", "same@example.com", null));

        var duplicateEmail = await service.RegisterAsync(new RegisterUserRequest("Outro Email", "DOC-40000", "same@example.com", null));

        Assert.IsInstanceOfType<UserAccountEmailDuplicateError>(GetOnlyError(duplicateEmail));
    }

    /// <summary>
    /// Proves that Either results become explicit HTTP status codes.
    /// </summary>
    [TestMethod]
    public void RegistrationEndpoint_MapsApplicationResultToHttpStatus()
    {
        var created = RegistrationEndpoint.ToHttpResult(
            Prelude.Right<Seq<DomainError>, RegisteredUserDto>(
                new RegisteredUserDto(Guid.NewGuid(), "Ada", "100", "ada@example.com", Prelude.None)));
        var badRequest = RegistrationEndpoint.ToHttpResult(
            Prelude.Left<Seq<DomainError>, RegisteredUserDto>(
                Prelude.Seq1<DomainError>(new EmailInvalidError())));
        var conflict = RegistrationEndpoint.ToHttpResult(
            Prelude.Left<Seq<DomainError>, RegisteredUserDto>(
                Prelude.Seq1<DomainError>(new UserAccountDocumentDuplicateError())));

        Assert.AreEqual(StatusCodes.Status201Created, GetStatus(created));
        Assert.AreEqual(StatusCodes.Status400BadRequest, GetStatus(badRequest));
        Assert.AreEqual(StatusCodes.Status409Conflict, GetStatus(conflict));
    }

    /// <summary>
    /// Proves that expected errors are exposed as RFC 7807 problem details with error extensions.
    /// </summary>
    [TestMethod]
    public void ProblemResult_ReturnsProblemDetailsWithErrorsExtension()
    {
        var result = ProblemResult.FromErrors(Prelude.Seq1<DomainError>(new EmailInvalidError()));

        var problem = Assert.IsInstanceOfType<ProblemHttpResult>(result);

        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.IsNotNull(problem.ProblemDetails);
        Assert.IsTrue(problem.ProblemDetails.Extensions.ContainsKey("errors"));
    }

    /// <summary>
    /// Proves that the concept demo exercises the LanguageExt.Core primitives used by the tutorial.
    /// </summary>
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

    /// <summary>
    /// Creates an isolated EF InMemory context for each test.
    /// </summary>
    private static RegistrationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RegistrationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new RegistrationDbContext(options);
    }

    /// <summary>
    /// Creates the command service through application ports and EF adapters.
    /// </summary>
    private static RegisterUserService CreateService(RegistrationDbContext dbContext)
    {
        return new RegisterUserService(
            new EfUserAccountLookup(dbContext),
            new EfUserAccountWriter(dbContext),
            dbContext);
    }

    /// <summary>
    /// Extracts the Right value or fails the test with the Left value.
    /// </summary>
    private static TRight GetRight<TLeft, TRight>(Either<TLeft, TRight> result)
    {
        return result.Match(
            Right: value => value,
            Left: error => throw new AssertFailedException($"Expected Right, got Left({error})."));
    }

    /// <summary>
    /// Extracts the Left value or fails the test with the Right value.
    /// </summary>
    private static TLeft GetLeft<TLeft, TRight>(Either<TLeft, TRight> result)
    {
        return result.Match(
            Right: value => throw new AssertFailedException($"Expected Left, got Right({value})."),
            Left: error => error);
    }

    /// <summary>
    /// Reads the only registration error expected by conflict validation tests.
    /// </summary>
    private static DomainError GetOnlyError(Either<Seq<DomainError>, RegisteredUserDto> result)
    {
        var errors = GetLeft(result).ToArray();

        Assert.HasCount(1, errors);
        return errors[0];
    }

    /// <summary>
    /// Reads the only domain error expected by aggregate result tests.
    /// </summary>
    private static DomainError GetOnlyError(Either<Seq<DomainError>, Unit> result)
    {
        var errors = GetLeft(result).ToArray();

        Assert.HasCount(1, errors);
        return errors[0];
    }

    /// <summary>
    /// Creates a valid aggregate for domain-event and persistence tests.
    /// </summary>
    private static UserAccount CreateValidAccount()
    {
        return GetRight(UserAccount.Create("Ada Lovelace", "DOC-10000", "ada@example.com", null));
    }

    /// <summary>
    /// Reads ASP.NET Core status code metadata from an IResult.
    /// </summary>
    private static int GetStatus(IResult result)
    {
        return result is IStatusCodeHttpResult statusCodeResult
            ? statusCodeResult.StatusCode ?? StatusCodes.Status200OK
            : StatusCodes.Status200OK;
    }
}
