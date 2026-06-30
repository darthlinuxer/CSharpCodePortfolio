using System.Reflection;
using System.Text.RegularExpressions;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Handlers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Handlers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Infrastructure.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Handlers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Messaging;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Messaging;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Functional;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;
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
    public async Task Identity_RegisterUser_ReturnsUserAccountDto()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateRegisterUserService(dbContext);

        var result = await service.RegisterAsync(new RegisterUserRequest("Grace Hopper", "grace@example.com", null));

        var user = Assert.IsInstanceOfType<UserAccountDto>(GetRight(result));
        Assert.AreEqual("Grace Hopper", user.Name);
        Assert.AreEqual("grace@example.com", user.Email);
    }

    [TestMethod]
    public async Task Identity_UpdateProfile_PersistsChanges()
    {
        await using var dbContext = CreateDbContext();
        var registered = await CreateRegisterUserService(dbContext)
            .RegisterAsync(new RegisterUserRequest("Grace Hopper", "grace@example.com", null));
        var service = CreateUpdateUserAccountProfileService(dbContext);

        var result = await service.UpdateAsync(new UpdateUserAccountProfileRequest(
            GetRight(registered).Id,
            "Amazing Grace",
            "amazing@example.com",
            "11988887777"));
        var user = await dbContext.Users.AsNoTracking().SingleAsync();

        Assert.IsTrue(result.IsRight);
        Assert.AreEqual("Amazing Grace", user.Name.Value);
        Assert.AreEqual("amazing@example.com", user.Email.Value);
        Assert.AreEqual("11988887777", user.PhoneNumber.Single().Value);
    }

    [TestMethod]
    public async Task Identity_UpdateProfile_RejectsUnknownUser()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateUpdateUserAccountProfileService(dbContext);

        var result = await service.UpdateAsync(new UpdateUserAccountProfileRequest(
            Guid.CreateVersion7(),
            "Grace Hopper",
            "grace@example.com",
            null));

        Assert.IsInstanceOfType<UserAccountNotFoundError>(GetOnlyError(result));
    }

    [TestMethod]
    public async Task Identity_UpdateProfile_RejectsDuplicateEmail()
    {
        await using var dbContext = CreateDbContext();
        var grace = await CreateRegisterUserService(dbContext)
            .RegisterAsync(new RegisterUserRequest("Grace Hopper", "grace@example.com", null));
        await CreateRegisterUserService(dbContext)
            .RegisterAsync(new RegisterUserRequest("Ada Lovelace", "ada@example.com", null));
        var service = CreateUpdateUserAccountProfileService(dbContext);

        var result = await service.UpdateAsync(new UpdateUserAccountProfileRequest(
            GetRight(grace).Id,
            "Grace Hopper",
            "ada@example.com",
            null));

        Assert.IsInstanceOfType<UserAccountEmailDuplicateError>(GetOnlyError(result));
    }

    [TestMethod]
    public async Task UnitOfWork_TranslatesUserEmailUniqueConstraintWithoutContextCoupling()
    {
        await using var dbContext = CreateDbContext();
        var writer = new EfUserAccountWriter(dbContext);
        writer.Add(CreateUserAccount("Grace Hopper", "grace@example.com"));
        writer.Add(CreateUserAccount("Ada Lovelace", "grace@example.com"));

        var result = await CreateUnitOfWork(dbContext).CommitAsync(CancellationToken.None);

        Assert.IsInstanceOfType<UserAccountEmailDuplicateError>(GetOnlyError(result));
    }

    [TestMethod]
    public void UserAccount_Create_AccumulatesValueObjectErrors()
    {
        var result = UserAccount.Create(None, Some("not-email"), Some("abc"), TimeProvider.System);
        var errors = GetLeft(result).ToArray();

        Assert.HasCount(3, errors);
        Assert.IsTrue(errors.Any(error => error is PersonNameRequiredError));
        Assert.IsTrue(errors.Any(error => error is EmailInvalidError));
        Assert.IsTrue(errors.Any(error => error is PhoneNumberInvalidError));
    }

    [TestMethod]
    public void UserAccount_EnsureEmailIsAvailable_ReturnsDuplicateError()
    {
        var result = UserAccount.EnsureEmailIsAvailable(emailAlreadyExists: true);

        Assert.IsInstanceOfType<UserAccountEmailDuplicateError>(GetLeft(result));
    }

    [TestMethod]
    public void UserAccount_EnsureEmailCanChangeTo_AllowsCurrentEmail()
    {
        var account = CreateUserAccount();

        var result = account.EnsureEmailCanChangeTo(account.Email, emailAlreadyExists: true);

        Assert.IsTrue(result.IsRight);
    }

    [TestMethod]
    public void UserAccount_Rename_ChangesNameAndPublishesEvent()
    {
        var account = CreateUserAccount();
        account.ClearDomainEvents();

        var result = account.Rename(GetRight(PersonName.Create(Some("Amazing Grace"))), TimeProvider.System);

        Assert.IsTrue(result.IsRight);
        Assert.AreEqual("Amazing Grace", account.Name.Value);
        Assert.IsInstanceOfType<UserAccountNameChangedDomainEvent>(account.DomainEvents.Single());
    }

    [TestMethod]
    public void UserAccount_Rename_WithSameName_DoesNotPublishEvent()
    {
        var account = CreateUserAccount();
        account.ClearDomainEvents();

        var result = account.Rename(account.Name, TimeProvider.System);

        Assert.IsTrue(result.IsRight);
        Assert.IsEmpty(account.DomainEvents);
    }

    [TestMethod]
    public void UserAccount_ChangeEmail_ChangesEmailAndPublishesEvent()
    {
        var account = CreateUserAccount();
        account.ClearDomainEvents();

        var result = account.ChangeEmail(GetRight(Email.Create(Some("amazing@example.com"))), TimeProvider.System);

        Assert.IsTrue(result.IsRight);
        Assert.AreEqual("amazing@example.com", account.Email.Value);
        Assert.IsInstanceOfType<UserAccountEmailChangedDomainEvent>(account.DomainEvents.Single());
    }

    [TestMethod]
    public void UserAccount_ChangePhoneNumber_ChangesPhoneAndPublishesEvent()
    {
        var account = CreateUserAccount();
        account.ClearDomainEvents();

        var result = account.ChangePhoneNumber(GetRight(PhoneNumber.CreateOptional(Some("11988887777"))), TimeProvider.System);

        Assert.IsTrue(result.IsRight);
        Assert.AreEqual("11988887777", account.PhoneNumber.Single().Value);
        Assert.IsInstanceOfType<UserAccountPhoneNumberChangedDomainEvent>(account.DomainEvents.Single());
    }

    [TestMethod]
    public async Task DomainEventBus_DispatchesOnlyHandlersFromSameBoundedContext()
    {
        var integrationEventBus = new RecordingIntegrationEventBus();
        var domainEventBus = new InMemoryDomainEventBus(
        [
            new DomainEventConsumer<UserAccountRegisteredDomainEvent>(
                new PublishUserRegisteredIntegrationEventHandler(integrationEventBus))
        ]);

        var result = await domainEventBus.PublishAsync(
            Seq<IDomainEvent>(
                NewUserAccountRegisteredDomainEvent(),
                NewOrderConfirmedDomainEvent()),
            CancellationToken.None);

        Assert.IsTrue(result.IsRight);
        Assert.HasCount(1, integrationEventBus.Events);
        Assert.IsInstanceOfType<UserRegisteredIntegrationEvent>(integrationEventBus.Events.Single());
    }

    [TestMethod]
    public async Task Identity_DomainEventHandler_PublishesUserRegisteredIntegrationEvent()
    {
        var integrationEventBus = new RecordingIntegrationEventBus();
        var handler = new PublishUserRegisteredIntegrationEventHandler(integrationEventBus);

        var result = await handler.HandleAsync(NewUserAccountRegisteredDomainEvent(), CancellationToken.None);

        var integrationEvent = Assert.IsInstanceOfType<UserRegisteredIntegrationEvent>(integrationEventBus.Events.Single());
        Assert.IsTrue(result.IsRight);
        Assert.AreEqual("grace@example.com", integrationEvent.Email);
    }

    [TestMethod]
    public async Task Ordering_DomainEventHandler_PublishesOrderConfirmedIntegrationEvent()
    {
        var integrationEventBus = new RecordingIntegrationEventBus();
        var handler = new PublishOrderConfirmedIntegrationEventHandler(integrationEventBus);

        var result = await handler.HandleAsync(NewOrderConfirmedDomainEvent(), CancellationToken.None);

        var integrationEvent = Assert.IsInstanceOfType<OrderConfirmedIntegrationEvent>(integrationEventBus.Events.Single());
        Assert.IsTrue(result.IsRight);
        Assert.AreEqual(120, integrationEvent.TotalAmount);
    }

    [TestMethod]
    public async Task IntegrationEventBus_StoresEventsInOutboxWithoutCallingConsumerDirectly()
    {
        await using var dbContext = CreateDbContext();
        var integrationEventBus = new OutboxIntegrationEventBus(dbContext);

        var result = await integrationEventBus.PublishAsync(NewUserRegisteredIntegrationEvent(), CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        Assert.IsTrue(result.IsRight);
        Assert.AreEqual(1, await dbContext.OutboxMessages.CountAsync());
        Assert.AreEqual(0, await dbContext.CustomerDirectory.CountAsync());
    }

    [TestMethod]
    public async Task OutboxDispatcher_DeliversIntegrationEventsToRegisteredConsumers()
    {
        await using var dbContext = CreateDbContext();
        var integrationEventBus = new OutboxIntegrationEventBus(dbContext);
        var integrationEvent = NewUserRegisteredIntegrationEvent();
        await integrationEventBus.PublishAsync(integrationEvent, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var dispatched = await CreateDispatcher(dbContext).DispatchPendingAsync(CancellationToken.None);
        var customers = await dbContext.CustomerDirectory.AsNoTracking().ToArrayAsync();

        Assert.AreEqual(1, dispatched);
        Assert.IsTrue(customers.Any(customer => customer.Id.Value == integrationEvent.UserAccountId));
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
        var handler = new CreateInvoiceWhenOrderConfirmedHandler(
            new EfInvoiceWriter(dbContext),
            CreateUnitOfWork(dbContext),
            TimeProvider.System);
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
    public void FunctionalExtensions_Combine_AccumulatesValueObjectErrors()
    {
        var result = (
            Sku.Create(None),
            Quantity.Create(0),
            Money.Create(0))
            .Combine((sku, quantity, money) => new OrderLineDraft(sku, quantity, money));

        var errors = GetLeft(result).ToArray();

        Assert.HasCount(3, errors);
        Assert.IsTrue(errors.Any(error => error is SkuRequiredError));
        Assert.IsTrue(errors.Any(error => error is QuantityPositiveRequiredError));
        Assert.IsTrue(errors.Any(error => error is MoneyPositiveRequiredError));
    }

    [TestMethod]
    public void FunctionalExtensions_Combine_ReturnsProjectedValue()
    {
        var result = (
            Sku.Create(Some("book-ddd")),
            Quantity.Create(2),
            Money.Create(30))
            .Combine((sku, quantity, money) => new OrderLineDraft(sku, quantity, money));

        var draft = GetRight(result);

        Assert.AreEqual("BOOK-DDD", draft.Sku.Value);
        Assert.AreEqual(2, draft.Quantity.Value);
        Assert.AreEqual(30, draft.UnitPrice.Value);
    }

    [TestMethod]
    public void FunctionalExtensions_Collect_AccumulatesErrors()
    {
        var results = new[]
        {
            Right<Seq<DomainError>, OrderLineDraft>(CreateOrderLineDraft("book-ddd")),
            Left<Seq<DomainError>, OrderLineDraft>(Seq1<DomainError>(new SkuRequiredError())),
            Left<Seq<DomainError>, OrderLineDraft>(Seq1<DomainError>(new QuantityPositiveRequiredError()))
        };

        var collected = results.Collect();
        var errors = GetLeft(collected).ToArray();

        Assert.HasCount(2, errors);
        Assert.IsTrue(errors.Any(error => error is SkuRequiredError));
        Assert.IsTrue(errors.Any(error => error is QuantityPositiveRequiredError));
    }

    [TestMethod]
    public void FunctionalExtensions_Ensure_ReturnsUnitOrError()
    {
        var right = true.Ensure(() => new SkuRequiredError());
        var left = false.Ensure(() => new SkuRequiredError());
        var rightSeq = true.EnsureSeq(() => new SkuRequiredError());
        var leftSeq = false.EnsureSeq(() => new SkuRequiredError());

        Assert.IsTrue(right.IsRight);
        Assert.IsInstanceOfType<SkuRequiredError>(GetLeft(left));
        Assert.IsTrue(rightSeq.IsRight);
        Assert.IsInstanceOfType<SkuRequiredError>(GetLeft(leftSeq).Single());
    }

    [TestMethod]
    public void FunctionalExtensions_ToNonBlankOption_MapsBlankToNone()
    {
        Assert.IsTrue(((string?)null).ToNonBlankOption().IsNone);
        Assert.IsTrue(string.Empty.ToNonBlankOption().IsNone);
        Assert.IsTrue("   ".ToNonBlankOption().IsNone);
        Assert.AreEqual(
            "Ada",
            "Ada".ToNonBlankOption().Match(
                Some: value => value,
                None: () => throw new AssertFailedException()));
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
    public void Architecture_ContextsDoNotReferenceOtherContexts()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30");
        var offenders = Directory.EnumerateFiles(Path.Combine(sourceRoot, "Contexts"), "*.cs", SearchOption.AllDirectories)
            .SelectMany(file => File.ReadLines(file)
                .Select((line, index) => new { File = file, Line = index + 1, Text = line }))
            .Where(item => CrossesAnotherContext(item.File, item.Text))
            .Select(item => $"{Path.GetRelativePath(sourceRoot, item.File)}:{item.Line}: {item.Text.Trim()}")
            .ToArray();

        Assert.IsEmpty(offenders, string.Join(Environment.NewLine, offenders));
    }

    [TestMethod]
    public void Architecture_DomainEventHandlersStayInsideTheirBoundedContext()
    {
        var handlerContract = typeof(IDomainEventHandler<>);
        var offenders = typeof(UserAccount).Assembly.GetTypes()
            .Where(type => type.Namespace?.Contains(".Contexts.", StringComparison.Ordinal) == true)
            .SelectMany(type => type.GetInterfaces()
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == handlerContract)
                .Select(contract => new
                {
                    Handler = type,
                    DomainEvent = contract.GetGenericArguments()[0]
                }))
            .Where(pair => ContextName(pair.Handler) != ContextName(pair.DomainEvent))
            .Select(pair => $"{pair.Handler.FullName} handles {pair.DomainEvent.FullName}")
            .ToArray();

        Assert.IsEmpty(offenders, string.Join(Environment.NewLine, offenders));
    }

    [TestMethod]
    public void Architecture_IntegrationMessagingDoesNotReferenceContexts()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30");
        var messagingRoot = Path.Combine(sourceRoot, "Integration", "Messaging");
        var offenders = Directory.EnumerateFiles(messagingRoot, "*.cs", SearchOption.AllDirectories)
            .SelectMany(file => File.ReadLines(file)
                .Select((line, index) => new { File = file, Line = index + 1, Text = line }))
            .Where(item => item.Text.Contains(".Contexts.", StringComparison.Ordinal))
            .Select(item => $"{Path.GetRelativePath(sourceRoot, item.File)}:{item.Line}: {item.Text.Trim()}")
            .ToArray();

        Assert.IsEmpty(offenders, string.Join(Environment.NewLine, offenders));
    }

    [TestMethod]
    public void Architecture_IntegrationMessagingContainsOnlyContracts()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30");
        var offenders = Directory.EnumerateFiles(Path.Combine(sourceRoot, "Integration", "Messaging"), "*.cs", SearchOption.AllDirectories)
            .SelectMany(file => File.ReadLines(file)
                .Select((line, index) => new { File = file, Line = index + 1, Text = line }))
            .Where(item => item.Text.Contains(" class ", StringComparison.Ordinal)
                || item.Text.Contains(".Infrastructure.", StringComparison.Ordinal)
                || item.Text.Contains(".Outbox", StringComparison.Ordinal)
                || item.Text.Contains("Tutorial30DbContext", StringComparison.Ordinal))
            .Select(item => $"{Path.GetRelativePath(sourceRoot, item.File)}:{item.Line}: {item.Text.Trim()}")
            .ToArray();

        Assert.IsEmpty(offenders, string.Join(Environment.NewLine, offenders));
    }

    [TestMethod]
    public void Architecture_InfrastructureMessagingDoesNotReferenceContexts()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30");
        var offenders = Directory.EnumerateFiles(Path.Combine(sourceRoot, "Infrastructure", "Messaging"), "*.cs", SearchOption.AllDirectories)
            .SelectMany(file => File.ReadLines(file)
                .Select((line, index) => new { File = file, Line = index + 1, Text = line }))
            .Where(item => item.Text.Contains(".Contexts.", StringComparison.Ordinal))
            .Select(item => $"{Path.GetRelativePath(sourceRoot, item.File)}:{item.Line}: {item.Text.Trim()}")
            .ToArray();

        Assert.IsEmpty(offenders, string.Join(Environment.NewLine, offenders));
    }

    [TestMethod]
    public void Architecture_UnitOfWorkDoesNotReferenceSpecificBoundedContexts()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30");
        var unitOfWork = Path.Combine(sourceRoot, "Infrastructure", "Persistence", "EfTutorial30UnitOfWork.cs");
        var offenders = File.ReadLines(unitOfWork)
            .Select((line, index) => new { Line = index + 1, Text = line })
            .Where(item => item.Text.Contains(".Contexts.", StringComparison.Ordinal))
            .Select(item => $"{Path.GetRelativePath(sourceRoot, unitOfWork)}:{item.Line}: {item.Text.Trim()}")
            .ToArray();

        Assert.IsEmpty(offenders, string.Join(Environment.NewLine, offenders));
    }

    [TestMethod]
    public void Architecture_CommandServicesDoNotPublishIntegrationEventsDirectly()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "CSharpCodePortfolio.Tutorials.Tutorial30");
        var offenders = Directory.EnumerateFiles(Path.Combine(sourceRoot, "Contexts"), "*.cs", SearchOption.AllDirectories)
            .Where(file => file.Contains($"{Path.DirectorySeparatorChar}Application{Path.DirectorySeparatorChar}Commands{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(file => File.ReadLines(file)
                .Select((line, index) => new { File = file, Line = index + 1, Text = line }))
            .Where(item => item.Text.Contains(".Integration.Events", StringComparison.Ordinal)
                || item.Text.Contains(".Integration.Messaging", StringComparison.Ordinal)
                || item.Text.Contains("IIntegrationEventBus", StringComparison.Ordinal)
                || item.Text.Contains("OutboxIntegrationEventBus", StringComparison.Ordinal))
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
            CreateUnitOfWork(dbContext),
            TimeProvider.System);

    private static UpdateUserAccountProfileService CreateUpdateUserAccountProfileService(Tutorial30DbContext dbContext) =>
        new(
            new EfUserAccountLookup(dbContext),
            new EfUserAccountWriter(dbContext),
            CreateUnitOfWork(dbContext),
            TimeProvider.System);

    private static PlaceOrderService CreatePlaceOrderService(Tutorial30DbContext dbContext) =>
        new(
            new EfCustomerDirectory(dbContext),
            new EfOrderWriter(dbContext),
            CreateUnitOfWork(dbContext),
            TimeProvider.System);

    private static ConfirmOrderService CreateConfirmOrderService(Tutorial30DbContext dbContext) =>
        new(new EfOrderWriter(dbContext), CreateUnitOfWork(dbContext), TimeProvider.System);

    private static InProcessOutboxDispatcher CreateDispatcher(Tutorial30DbContext dbContext) =>
        new(
            dbContext,
            [
                new OutboxIntegrationEventConsumer<UserRegisteredIntegrationEvent, Unit>(
                    UserRegisteredIntegrationEvent.EventType,
                    new RegisterCustomerWhenUserRegisteredHandler(new EfCustomerDirectory(dbContext), CreateUnitOfWork(dbContext))),
                new OutboxIntegrationEventConsumer<OrderConfirmedIntegrationEvent, InvoiceHandlingResult>(
                    OrderConfirmedIntegrationEvent.EventType,
                    new CreateInvoiceWhenOrderConfirmedHandler(
                        new EfInvoiceWriter(dbContext),
                        CreateUnitOfWork(dbContext),
                        TimeProvider.System))
            ],
            TimeProvider.System);

    private static EfTutorial30UnitOfWork CreateUnitOfWork(Tutorial30DbContext dbContext) =>
        new(dbContext, CreateDomainEventBus(dbContext), [new IdentityPersistenceErrorTranslator()]);

    private static InMemoryDomainEventBus CreateDomainEventBus(Tutorial30DbContext dbContext)
    {
        var integrationEventBus = new OutboxIntegrationEventBus(dbContext);

        return new InMemoryDomainEventBus(
        [
            new DomainEventConsumer<UserAccountRegisteredDomainEvent>(
                new PublishUserRegisteredIntegrationEventHandler(integrationEventBus)),
            new DomainEventConsumer<OrderConfirmedDomainEvent>(
                new PublishOrderConfirmedIntegrationEventHandler(integrationEventBus))
        ]);
    }

    private static async Task<UserAccountDto> RegisterAndDispatchCustomerAsync(Tutorial30DbContext dbContext)
    {
        var registered = await CreateRegisterUserService(dbContext)
            .RegisterAsync(new RegisterUserRequest("Grace Hopper", "grace@example.com", null));
        await CreateDispatcher(dbContext).DispatchPendingAsync(CancellationToken.None);

        return GetRight(registered);
    }

    private static UserAccount CreateUserAccount() =>
        CreateUserAccount("Grace Hopper", "grace@example.com");

    private static UserAccount CreateUserAccount(string name, string email) =>
        GetRight(UserAccount.Create(
            Some(name),
            Some(email),
            None,
            TimeProvider.System));

    private static UserAccountRegisteredDomainEvent NewUserAccountRegisteredDomainEvent() =>
        new(
            Guid.CreateVersion7(),
            GetRight(Email.Create(Some("grace@example.com"))),
            Timestamp.UtcNow(TimeProvider.System));

    private static UserRegisteredIntegrationEvent NewUserRegisteredIntegrationEvent() =>
        new(
            Guid.CreateVersion7(),
            Timestamp.UtcNow(TimeProvider.System),
            Guid.CreateVersion7(),
            "grace@example.com");

    private static OrderConfirmedDomainEvent NewOrderConfirmedDomainEvent() =>
        new(
            GetRight(OrderId.Create(Guid.CreateVersion7())),
            GetRight(CustomerId.Create(Guid.CreateVersion7())),
            GetRight(Money.Create(120)),
            Timestamp.UtcNow(TimeProvider.System));

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

    private static string ContextName(Type type) =>
        (type.Namespace ?? string.Empty)
        .Split(".Contexts.", StringSplitOptions.None)
        .Last()
        .Split('.')[0];

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

    private sealed class RecordingIntegrationEventBus : IIntegrationEventBus
    {
        private readonly List<IIntegrationEvent> events = [];

        public IReadOnlyCollection<IIntegrationEvent> Events => events;

        public Task<Either<Seq<DomainError>, Unit>> PublishAsync(
            IIntegrationEvent integrationEvent,
            CancellationToken cancellationToken)
        {
            events.Add(integrationEvent);
            return Task.FromResult(Right<Seq<DomainError>, Unit>(default));
        }
    }
}
