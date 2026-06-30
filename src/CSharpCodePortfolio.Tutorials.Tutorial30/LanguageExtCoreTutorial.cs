using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Application.Handlers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Handlers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Infrastructure.Persistence.ConfigurationMappings;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Infrastructure.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Handlers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Application.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Customers;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Messaging;
using CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Traditional;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30;

/// <summary>
/// Runs the LanguageExt.Core tutorial as a small monadic DDD skeleton with bounded contexts.
/// </summary>
[Tutorial("30", "language-ext-core", "LanguageExt.Core + DDD monádico")]
public sealed class LanguageExtCoreTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("30", "LanguageExt.Core + DDD monádico");
        TutorialConsole.WriteContext(
            ("Pacote", "LanguageExt.Core 4.4.9"),
            ("Bounded contexts", "Identity, Ordering, Billing"),
            ("Comunicação", "Domain Event -> Integration Event -> Outbox -> handler in-process"),
            ("Persistência", "EF Core 10 SQLite em memória + value converters"));

        TutorialConsole.WriteQuestion(
            "Como modelar aggregates independentes, comunicação entre bounded contexts e erros esperados sem null/exception como fluxo de negócio?");

        TutorialConsole.WriteHypothesis(
            "Cada bounded context protege sua linguagem e seus invariantes.",
            "Aggregates externos entram por IDs/value objects locais, nunca por referência direta.",
            "Integration events são contratos publicados; o outbox dá atomicidade sem broker real no tutorial.");

        TutorialConsole.WriteExperiment(
            1,
            "Anti-exemplo tradicional",
            "Null, exception e condicionais manuais fazem o fluxo depender de convenção humana.");
        TutorialConsole.WriteCodeSnippet(
            "Service tradicional",
            typeof(TraditionalNullRegistrationExample.TraditionalRegistrationService),
            nameof(TraditionalNullRegistrationExample.TraditionalRegistrationService.RegisterAsync));

        TutorialConsole.WriteExperiment(
            2,
            "Identity cria UserAccount e publica contrato",
            "UserAccount continua aggregate root rico; registro bem-sucedido grava UserRegisteredIntegrationEvent no outbox.");
        TutorialConsole.WriteCodeSnippet("UserAccount factory", typeof(UserAccount), nameof(UserAccount.Create));
        TutorialConsole.WriteCodeSnippet("Identity mapping", typeof(UserAccountConfiguration), nameof(UserAccountConfiguration.Configure));

        await using var dbContext = CreateDbContext();
        var registerUser = new RegisterUserService(
            new EfUserAccountLookup(dbContext),
            new EfUserAccountWriter(dbContext),
            CreateUnitOfWork(dbContext),
            TimeProvider.System);
        var registered = await registerUser.RegisterAsync(
            new RegisterUserRequest("Grace Hopper", "grace@example.com", "11999998888"),
            cancellationToken).ConfigureAwait(false);

        var dispatcher = CreateDispatcher(dbContext);
        var identityDispatches = await dispatcher.DispatchPendingAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Identity -> Ordering",
            ("Registro", ToIdentityResult(registered)),
            ("Outbox entregue", identityDispatches.ToString()),
            ("Clientes Ordering", (await dbContext.CustomerDirectory.CountAsync(cancellationToken).ConfigureAwait(false)).ToString()));

        TutorialConsole.WriteExperiment(
            3,
            "Ordering referencia cliente por CustomerId",
            "Order não conhece UserAccount; cliente vem da projeção local mantida por evento de integração.");
        TutorialConsole.WriteCodeSnippet("Order.Place", typeof(Order), nameof(Order.Place));
        TutorialConsole.WriteCodeSnippet("PlaceOrderService", typeof(PlaceOrderService), nameof(PlaceOrderService.PlaceAsync));

        var customerId = GetRight(registered).Id;
        var placeOrder = new PlaceOrderService(
            new EfCustomerDirectory(dbContext),
            new EfOrderWriter(dbContext),
            CreateUnitOfWork(dbContext),
            TimeProvider.System);
        var unknownCustomer = await placeOrder.PlaceAsync(
            new PlaceOrderRequest(Guid.CreateVersion7(), [new PlaceOrderLineRequest("book-ddd", 1, 120)]),
            cancellationToken).ConfigureAwait(false);
        var placed = await placeOrder.PlaceAsync(
            new PlaceOrderRequest(customerId, [new PlaceOrderLineRequest("book-ddd", 2, 120)]),
            cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Ordering",
            ("Cliente desconhecido", ToOrderResult(unknownCustomer)),
            ("Pedido criado", ToOrderResult(placed)),
            ("Linhas", (await dbContext.Orders.SelectMany(order => order.Lines).CountAsync(cancellationToken).ConfigureAwait(false)).ToString()));

        TutorialConsole.WriteExperiment(
            4,
            "OrderConfirmed vira Invoice no Billing",
            "Ordering publica OrderConfirmedIntegrationEvent; Billing traduz o contrato e cria Invoice de forma idempotente.");
        TutorialConsole.WriteCodeSnippet("ConfirmOrderService", typeof(ConfirmOrderService), nameof(ConfirmOrderService.ConfirmAsync));
        TutorialConsole.WriteCodeSnippet("Billing handler", typeof(CreateInvoiceWhenOrderConfirmedHandler), nameof(CreateInvoiceWhenOrderConfirmedHandler.HandleAsync));

        var confirmOrder = new ConfirmOrderService(
            new EfOrderWriter(dbContext),
            CreateUnitOfWork(dbContext),
            TimeProvider.System);
        var confirmed = await confirmOrder.ConfirmAsync(
            new ConfirmOrderRequest(GetRight(placed).Id),
            cancellationToken).ConfigureAwait(false);
        var billingDispatches = await dispatcher.DispatchPendingAsync(cancellationToken).ConfigureAwait(false);
        var repeatedDispatches = await dispatcher.DispatchPendingAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Ordering -> Billing",
            ("Pedido confirmado", ToConfirmedOrderResult(confirmed)),
            ("Eventos entregues", billingDispatches.ToString()),
            ("Reentrega idempotente", repeatedDispatches.ToString()),
            ("Invoices", (await dbContext.Invoices.CountAsync(cancellationToken).ConfigureAwait(false)).ToString()));

        TutorialConsole.WriteExperiment(
            5,
            "HTTP continua borda fina",
            "Presentation só traduz Either para status HTTP; regra fica em Domain/Application.");
        var conflict = RegistrationEndpoint.ToHttpResult(
            Left<Seq<DomainError>, RegisteredUserDto>(
                Seq1<DomainError>(new Contexts.Identity.Domain.Aggregates.UserAccounts.Errors.UserAccountEmailDuplicateError())));
        TutorialConsole.WriteEvidence(
            "HTTP",
            ("Status conflito", GetStatusCode(conflict).ToString()),
            ("Entidades mapeadas", string.Join(", ", dbContext.Model.GetEntityTypes().Select(entity => entity.ClrType.Name).Order())));

        TutorialConsole.WriteConclusion(
            "O tutorial agora mostra Strategic DDD mínimo: contexts separados, aggregates independentes, IDs entre contexts, published language, ACL, outbox e handlers.",
            TutorialConclusionKind.Success);
    }

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

    private static InProcessOutboxDispatcher CreateDispatcher(Tutorial30DbContext dbContext)
    {
        var unitOfWork = CreateUnitOfWork(dbContext);
        var customerHandler = new RegisterCustomerWhenUserRegisteredHandler(new EfCustomerDirectory(dbContext), unitOfWork);
        var billingHandler = new CreateInvoiceWhenOrderConfirmedHandler(
            new EfInvoiceWriter(dbContext),
            unitOfWork,
            TimeProvider.System);

        return new InProcessOutboxDispatcher(
            dbContext,
            [
                new OutboxIntegrationEventConsumer<UserRegisteredIntegrationEvent, Unit>(
                    UserRegisteredIntegrationEvent.EventType,
                    customerHandler),
                new OutboxIntegrationEventConsumer<OrderConfirmedIntegrationEvent, InvoiceHandlingResult>(
                    OrderConfirmedIntegrationEvent.EventType,
                    billingHandler)
            ],
            TimeProvider.System);
    }

    private static EfTutorial30UnitOfWork CreateUnitOfWork(Tutorial30DbContext dbContext) =>
        new(dbContext, CreateDomainEventBus(dbContext));

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

    private static string ToIdentityResult(Either<Seq<DomainError>, RegisteredUserDto> result) =>
        result.Match(
            Right: user => $"Right({user.Email})",
            Left: errors => $"Left({string.Join(", ", errors.Map(error => error.Code.ToString()))})");

    private static string ToOrderResult(Either<Seq<DomainError>, PlacedOrderDto> result) =>
        result.Match(
            Right: order => $"Right({order.TotalAmount:0.00})",
            Left: errors => $"Left({string.Join(", ", errors.Map(error => error.Code.ToString()))})");

    private static string ToConfirmedOrderResult(Either<Seq<DomainError>, ConfirmedOrderDto> result) =>
        result.Match(
            Right: order => $"Right({order.TotalAmount:0.00})",
            Left: errors => $"Left({string.Join(", ", errors.Map(error => error.Code.ToString()))})");

    private static TRight GetRight<TLeft, TRight>(Either<TLeft, TRight> result) =>
        result.Match(
            Right: value => value,
            Left: error => throw new InvalidOperationException($"Expected Right, got Left({error})."));

    private static int GetStatusCode(IResult result) =>
        result is IStatusCodeHttpResult statusCodeResult
            ? statusCodeResult.StatusCode ?? StatusCodes.Status200OK
            : StatusCodes.Status200OK;
}
