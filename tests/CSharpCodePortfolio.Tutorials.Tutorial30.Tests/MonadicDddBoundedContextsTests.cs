using System.Reflection;
using System.Text.RegularExpressions;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Handlers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Infrastructure.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Verifies Tutorial30 as a small monadic DDD skeleton with bounded contexts.
/// </summary>
[TestClass]
public sealed class MonadicDddBoundedContextsTests
{
    private static readonly string[] IgnoredSourceSegments =
    [
        $"{Path.DirectorySeparatorChar}Traditional{Path.DirectorySeparatorChar}",
        $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
        $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"
    ];

    private static readonly Regex NonCodeRegex = new(
        "(?s)/\\*.*?\\*/|//[^\\r\\n]*|\\\"\\\"\\\".*?\\\"\\\"\\\"|\\\"(?:\\\\.|[^\\\"\\\\])*\\\"|'(?:\\\\.|[^'\\\\])*'",
        RegexOptions.Compiled);

    private static readonly Regex ForbiddenBranchRegex = new(@"\b(" + "i" + "f" + "|" + "s" + "witch" + @")\b", RegexOptions.Compiled);

    [TestMethod]
    public async Task Identity_RegisterUser_PublishesUserRegisteredIntegrationEvent()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateRegisterUserService(dbContext);

        var result = await service.RegisterAsync(new RegisterUserRequest("Grace Hopper", "grace@example.com", null));

        Assert.IsTrue(result.IsRight);
        var messages = await dbContext.OutboxMessages.ToArrayAsync();
        Assert.HasCount(1, messages);
        Assert.AreEqual(UserRegisteredIntegrationEvent.EventType, messages[0].Type);
        Assert.IsTrue(messages[0].ProcessedAtUtc.IsNone);
    }

    [TestMethod]
    public async Task Ordering_PlaceOrder_UsesCustomerIdOnly_NoUserAccountReference()
    {
        await using var dbContext = CreateDbContext();
        var registered = await RegisterAndDispatchCustomerAsync(dbContext);
        var placeOrder = CreatePlaceOrderService(dbContext);

        var result = await placeOrder.PlaceAsync(new PlaceOrderRequest(
            registered.Id,
            [new PlaceOrderLineRequest("book-ddd", 1, 100)]));

        var order = GetRight(result);
        Assert.AreEqual(registered.Id, order.CustomerId);
        Assert.IsNull(typeof(Order).GetProperty(nameof(UserAccount)));
        Assert.IsFalse(typeof(Order)
            .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(member => member.ToString() ?? string.Empty)
            .Any(signature => signature.Contains(nameof(UserAccount), StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task Ordering_RejectsUnknownCustomer_WithDomainError()
    {
        await using var dbContext = CreateDbContext();
        var service = CreatePlaceOrderService(dbContext);

        var result = await service.PlaceAsync(new PlaceOrderRequest(
            Guid.CreateVersion7(),
            [new PlaceOrderLineRequest("book-ddd", 1, 100)]));

        Assert.IsInstanceOfType<UnknownCustomerError>(GetOnlyError(result));
    }

    [TestMethod]
    public void Order_Confirm_PublishesOrderConfirmedDomainEvent()
    {
        var order = CreateOrder();
        order.ClearDomainEvents();

        var result = order.Confirm(TimeProvider.System);

        Assert.IsTrue(result.IsRight);
        Assert.AreEqual(OrderStatus.Confirmed, order.Status);
        Assert.IsInstanceOfType<OrderConfirmedDomainEvent>(order.DomainEvents.Single());
    }

    [TestMethod]
    public void Order_AddLine_PublishesOrderLineAddedDomainEvent()
    {
        var order = CreateOrder();
        order.ClearDomainEvents();

        var result = order.AddLine(CreateOrderLineDraft("book-refactoring"), TimeProvider.System);

        Assert.IsTrue(result.IsRight);
        Assert.HasCount(2, order.Lines);
        Assert.IsInstanceOfType<OrderLineAddedDomainEvent>(order.DomainEvents.Single());
    }

    [TestMethod]
    public void Order_RemoveLine_PublishesOrderLineRemovedDomainEvent()
    {
        var order = CreateOrder();
        GetRight(order.AddLine(CreateOrderLineDraft("book-refactoring"), TimeProvider.System));
        var lineId = order.Lines.Last().Id;
        order.ClearDomainEvents();

        var result = order.RemoveLine(lineId, TimeProvider.System);

        Assert.IsTrue(result.IsRight);
        Assert.HasCount(1, order.Lines);
        Assert.IsInstanceOfType<OrderLineRemovedDomainEvent>(order.DomainEvents.Single());
    }

    [TestMethod]
    public void Order_RejectsChangingConfirmedOrCancelledOrder()
    {
        var confirmed = CreateOrder();
        GetRight(confirmed.Confirm(TimeProvider.System));
        var cancelled = CreateOrder();
        GetRight(cancelled.Cancel(TimeProvider.System));

        var confirmedChange = confirmed.AddLine(CreateOrderLineDraft("book-refactoring"), TimeProvider.System);
        var cancelledChange = cancelled.RemoveLine(cancelled.Lines.Single().Id, TimeProvider.System);

        Assert.IsInstanceOfType<OrderCannotBeChangedError>(GetLeft(confirmedChange));
        Assert.IsInstanceOfType<OrderCannotBeChangedError>(GetLeft(cancelledChange));
    }

    [TestMethod]
    public void Order_RejectsRemovingUnknownLine()
    {
        var order = CreateOrder();

        var result = order.RemoveLine(Guid.CreateVersion7(), TimeProvider.System);

        Assert.IsInstanceOfType<OrderLineNotFoundError>(GetLeft(result));
    }

    [TestMethod]
    public void Order_RejectsRemovingLastLine()
    {
        var order = CreateOrder();

        var result = order.RemoveLine(order.Lines.Single().Id, TimeProvider.System);

        Assert.IsInstanceOfType<OrderLineRequiredError>(GetLeft(result));
    }

    [TestMethod]
    public async Task Outbox_StoresIntegrationEventAfterSuccessfulCommit()
    {
        await using var dbContext = CreateDbContext();
        var registered = await RegisterAndDispatchCustomerAsync(dbContext);
        var placed = await CreatePlaceOrderService(dbContext).PlaceAsync(new PlaceOrderRequest(
            registered.Id,
            [new PlaceOrderLineRequest("book-ddd", 2, 120)]));
        var service = CreateConfirmOrderService(dbContext);

        var result = await service.ConfirmAsync(new ConfirmOrderRequest(GetRight(placed).Id));

        Assert.IsTrue(result.IsRight);
        var pending = (await dbContext.OutboxMessages.ToArrayAsync())
            .Count(message => message.Type == OrderConfirmedIntegrationEvent.EventType && message.ProcessedAtUtc.IsNone);
        Assert.AreEqual(1, pending);
    }

    [TestMethod]
    public async Task Billing_ConsumesOrderConfirmedOnce_CreatesSingleInvoice()
    {
        await using var dbContext = CreateDbContext();
        var registered = await RegisterAndDispatchCustomerAsync(dbContext);
        var placed = await CreatePlaceOrderService(dbContext).PlaceAsync(new PlaceOrderRequest(
            registered.Id,
            [new PlaceOrderLineRequest("book-ddd", 2, 120)]));
        await CreateConfirmOrderService(dbContext).ConfirmAsync(new ConfirmOrderRequest(GetRight(placed).Id));
        var dispatcher = CreateDispatcher(dbContext);

        var firstDispatch = await dispatcher.DispatchPendingAsync(CancellationToken.None);
        var secondDispatch = await dispatcher.DispatchPendingAsync(CancellationToken.None);

        Assert.AreEqual(1, firstDispatch);
        Assert.AreEqual(0, secondDispatch);
        Assert.AreEqual(1, await dbContext.Invoices.CountAsync());
    }

    [TestMethod]
    public async Task Billing_HandleAsync_ReturnsCreatedThenAlreadyHandled()
    {
        await using var dbContext = CreateDbContext();
        var handler = new CreateInvoiceWhenOrderConfirmedHandler(new EfInvoiceWriter(dbContext), dbContext, TimeProvider.System);
        var integrationEvent = new OrderConfirmedIntegrationEvent(
            Guid.CreateVersion7(),
            CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects.Timestamp.UtcNow(TimeProvider.System),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            120);

        var first = await handler.HandleAsync(integrationEvent, CancellationToken.None);
        var second = await handler.HandleAsync(integrationEvent, CancellationToken.None);

        Assert.IsInstanceOfType<InvoiceHandlingResult.Created>(GetRight(first));
        Assert.IsInstanceOfType<InvoiceHandlingResult.AlreadyHandled>(GetRight(second));
        Assert.AreEqual(1, await dbContext.Invoices.CountAsync());
    }

    [TestMethod]
    public void Architecture_DomainDoesNotReferenceOtherContextsInfrastructureOrPresentation()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30", "Contexts");
        var offenders = Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file => file.Contains($"{Path.DirectorySeparatorChar}Domain{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(file => File.ReadLines(file)
                .Select((line, index) => new { File = file, Line = index + 1, Text = line }))
            .Where(item => item.Text.Contains(".Infrastructure", StringComparison.Ordinal)
                || item.Text.Contains(".Application", StringComparison.Ordinal)
                || item.Text.Contains(".Presentation", StringComparison.Ordinal)
                || CrossesAnotherContext(item.File, item.Text))
            .Select(item => $"{Path.GetRelativePath(sourceRoot, item.File)}:{item.Line}: {item.Text.Trim()}")
            .ToArray();

        Assert.IsEmpty(offenders, string.Join(Environment.NewLine, offenders));
    }

    [TestMethod]
    public void Architecture_CrossContextContractsUseIntegrationEventsOnly()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30");
        var offenders = Directory.EnumerateFiles(Path.Combine(sourceRoot, "Contexts"), "*.cs", SearchOption.AllDirectories)
            .SelectMany(file => File.ReadLines(file)
                .Select((line, index) => new { File = file, Line = index + 1, Text = line }))
            .Where(item => CrossesContextWithoutIntegrationContract(item.File, item.Text))
            .Select(item => $"{Path.GetRelativePath(sourceRoot, item.File)}:{item.Line}: {item.Text.Trim()}")
            .ToArray();

        Assert.IsEmpty(offenders, string.Join(Environment.NewLine, offenders));
    }

    [TestMethod]
    public void Architecture_EntityDoesNotExposeDomainEvents()
    {
        Assert.IsNull(typeof(IEntity<>).GetProperty(nameof(IAggregate<object, object>.DomainEvents)));
        Assert.IsNotNull(typeof(IAggregate<,>).GetProperty(nameof(IAggregate<object, object>.DomainEvents)));
    }

    [TestMethod]
    public void Architecture_DddSkeletonDoesNotUseIfOrSwitchOutsideTraditional()
    {
        var offenders = Tutorial30SourceFiles()
            .SelectMany(ForbiddenTokensIn)
            .ToArray();

        Assert.IsEmpty(offenders, string.Join(Environment.NewLine, offenders));
    }

    private static RegisterUserService CreateRegisterUserService(Tutorial30DbContext dbContext) =>
        new(
            new EfUserAccountLookup(dbContext),
            new EfUserAccountWriter(dbContext),
            new EfIntegrationOutbox(dbContext),
            dbContext,
            TimeProvider.System);

    private static PlaceOrderService CreatePlaceOrderService(Tutorial30DbContext dbContext) =>
        new(
            new EfCustomerDirectory(dbContext),
            new EfOrderWriter(dbContext),
            dbContext,
            TimeProvider.System);

    private static ConfirmOrderService CreateConfirmOrderService(Tutorial30DbContext dbContext) =>
        new(new EfOrderWriter(dbContext), new EfIntegrationOutbox(dbContext), dbContext, TimeProvider.System);

    private static InProcessOutboxDispatcher CreateDispatcher(Tutorial30DbContext dbContext) =>
        new(
            dbContext,
            new RegisterCustomerWhenUserRegisteredHandler(new EfCustomerDirectory(dbContext), dbContext),
            new CreateInvoiceWhenOrderConfirmedHandler(new EfInvoiceWriter(dbContext), dbContext, TimeProvider.System),
            TimeProvider.System);

    private static async Task<RegisteredUserDto> RegisterAndDispatchCustomerAsync(Tutorial30DbContext dbContext)
    {
        var registered = await CreateRegisterUserService(dbContext)
            .RegisterAsync(new RegisterUserRequest("Grace Hopper", "grace@example.com", null));
        await CreateDispatcher(dbContext).DispatchPendingAsync(CancellationToken.None);

        return GetRight(registered);
    }

    private static Order CreateOrder()
    {
        return GetRight(Order.Place(
            GetRight(CustomerId.Create(Guid.CreateVersion7())),
            Seq1(CreateOrderLineDraft("book-ddd")),
            TimeProvider.System));
    }

    private static OrderLineDraft CreateOrderLineDraft(string sku) =>
        new(
            GetRight(Sku.Create(Some(sku))),
            GetRight(Quantity.Create(1)),
            GetRight(Money.Create(100)));

    private static Tutorial30DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Tutorial30DbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        var dbContext = new Tutorial30DbContext(options);
        dbContext.Database.OpenConnection();
        dbContext.Database.EnsureCreated();

        return dbContext;
    }

    private static DomainError GetOnlyError<T>(Either<Seq<DomainError>, T> result)
    {
        var errors = GetLeft(result).ToArray();
        Assert.HasCount(1, errors);
        return errors[0];
    }

    private static TRight GetRight<TLeft, TRight>(Either<TLeft, TRight> result) =>
        result.Match(
            Right: value => value,
            Left: error => throw new AssertFailedException($"Expected Right, got Left({error})."));

    private static TLeft GetLeft<TLeft, TRight>(Either<TLeft, TRight> result) =>
        result.Match(
            Right: value => throw new AssertFailedException($"Expected Left, got Right({value})."),
            Left: error => error);

    private static bool CrossesAnotherContext(string file, string text)
    {
        var context = file.Contains($"{Path.DirectorySeparatorChar}Identity{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            ? "Identity"
            : file.Contains($"{Path.DirectorySeparatorChar}Ordering{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                ? "Ordering"
                : "Billing";

        return context != "Identity" && text.Contains(".Contexts.Identity.", StringComparison.Ordinal)
            || context != "Ordering" && text.Contains(".Contexts.Ordering.", StringComparison.Ordinal)
            || context != "Billing" && text.Contains(".Contexts.Billing.", StringComparison.Ordinal);
    }

    private static bool CrossesContextWithoutIntegrationContract(string file, string text) =>
        !UsesIntegrationContract(file, text) && (
            file.Contains($"{Path.DirectorySeparatorChar}Ordering{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            && text.Contains(".Contexts.Identity.", StringComparison.Ordinal)
            || file.Contains($"{Path.DirectorySeparatorChar}Billing{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            && (text.Contains(".Contexts.Identity.", StringComparison.Ordinal)
                || text.Contains(".Contexts.Ordering.Domain.", StringComparison.Ordinal)));

    private static bool UsesIntegrationContract(string file, string text) =>
        file.Contains($"{Path.DirectorySeparatorChar}Integration{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || text.Contains(".Integration.Events", StringComparison.Ordinal);

    private static IEnumerable<string> Tutorial30SourceFiles() =>
        Directory.EnumerateFiles(Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30"), "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(FindRepositoryRoot(), "tests", "CSharpCodePortfolio.Tutorials.Tutorial30.Tests"), "*.cs", SearchOption.AllDirectories))
            .Where(IsScannedFile);

    private static bool IsScannedFile(string file) =>
        IgnoredSourceSegments.All(segment => !file.Contains(segment, StringComparison.Ordinal));

    private static IEnumerable<string> ForbiddenTokensIn(string file)
    {
        var code = RemoveNonCode(File.ReadAllText(file));
        return ForbiddenBranchRegex.Matches(code)
            .Select(match => $"{Path.GetRelativePath(FindRepositoryRoot(), file)}:{LineNumber(code, match.Index)}:{match.Value}");
    }

    private static string RemoveNonCode(string text) =>
        NonCodeRegex.Replace(text, match => new string('\n', match.Value.Count(character => character == '\n')));

    private static int LineNumber(string text, int index) =>
        text.Take(index).Count(character => character == '\n') + 1;

    private static string FindRepositoryRoot() =>
        ParentDirectories(new DirectoryInfo(AppContext.BaseDirectory))
            .Select(directory => new { directory, Marker = Path.Combine(directory.FullName, "CSharpCodePortfolio.slnx") })
            .Where(candidate => File.Exists(candidate.Marker))
            .Select(candidate => candidate.directory.FullName)
            .FirstOrDefault()
        ?? throw new AssertFailedException("Could not find repository root.");

    private static IEnumerable<DirectoryInfo> ParentDirectories(DirectoryInfo start)
    {
        var current = start;

        while (current is not null)
        {
            yield return current;
            current = current.Parent;
        }
    }
}
