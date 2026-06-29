using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence.ConfigurationMappings;
using CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http;
using CSharpCodePortfolio.Tutorials.Tutorial30.Traditional;
using CSharpCodePortfolio.Tutorials.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial30;

/// <summary>
/// Runs the LanguageExt.Core tutorial from a null-heavy registration flow to a composable functional flow.
/// </summary>
[Tutorial("30", "language-ext-core", "LanguageExt.Core pragmático")]
public sealed class LanguageExtCoreTutorial : ITutorial
{
    /// <summary>
    /// Executes the scenario, writes code snippets, and prints runtime evidence for the tutorial.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("30", "LanguageExt.Core pragmático");
        TutorialConsole.WriteContext(
            ("Pacote", "LanguageExt.Core 4.4.9"),
            ("Prerelease atual", "5.0.0-beta-77"),
            ("Decisão", "Stable 4.4.9 para evitar mudanças de superfície da v5 beta"),
            ("Persistência", "EF Core 10 SQLite em memória + ComplexProperty/HasConversion"));

        TutorialConsole.WriteQuestion(
            "Como modelar cadastro de usuário sem null checks espalhados, if/else procedural e exceptions para regra esperada?");

        TutorialConsole.WriteHypothesis(
            "Option<T> torna ausência válida explícita.",
            "Either<DomainError,T> deixa regra esperada no tipo de retorno.",
            "Seq<T>, Map, Bind, Match e LINQ query syntax compõem validação, duplicidade, persistência e HTTP sem estados implícitos.");

        TutorialConsole.WritePreparation(
            "O anti-exemplo mostra string?, Email?, PhoneNumber?, throws e service retornando null.",
            "O fluxo funcional usa value objects fechados, Option para telefone opcional e Either para falhas esperadas.",
            "EF Core mapeia UserAccount diretamente; Option<T> fica na API de domínio, não no shape de persistência.",
            "Try fica nas bordas técnicas; no LanguageExt.Core 4.4.9 executar Try<T> retorna Result<T>, não Fin<T>.");

        TutorialConsole.WriteExperiment(
            1,
            "C# tradicional deixa o estado inválido escapar",
            "Leia o service tradicional: null, throw e if/else carregam significado que o compilador não consegue proteger.");
        TutorialConsole.WriteCodeSnippet(
            "Anti-exemplo tradicional | service",
            typeof(TraditionalNullRegistrationExample.TraditionalRegistrationService),
            nameof(TraditionalNullRegistrationExample.TraditionalRegistrationService.RegisterAsync));
        TutorialConsole.WriteCodeSnippet(
            "Anti-exemplo tradicional | controller",
            typeof(TraditionalNullRegistrationExample.TraditionalRegistrationController),
            nameof(TraditionalNullRegistrationExample.TraditionalRegistrationController.RegisterAsync));

        TutorialConsole.WriteExperiment(
            2,
            "Value objects e Option removem nullables do domínio",
            "Mantenha Name e Email obrigatórios; use Option<T> só para ausência válida, como PhoneNumber.");
        TutorialConsole.WriteCodeSnippet(
            "Aggregate válido por construção",
            typeof(UserAccount),
            nameof(UserAccount.Create));

        TutorialConsole.WriteEvidence(
            "Aggregate DDD",
            ("Domain event", CreateDomainEventEvidence()));

        var concepts = LanguageExtConceptsDemo.Run();
        TutorialConsole.WriteEvidence(
            "Tipos centrais do LanguageExt.Core",
            ("Option None", concepts.AbsentEmailIsNone.ToString()),
            ("Either Right", concepts.NormalizedEmail),
            ("Map/Bind/LINQ", concepts.ComposedLength.ToString()),
            ("Fin", concepts.FinMessage),
            ("Try", concepts.TryValue.ToString()),
            ("Seq", concepts.SeqTotal.ToString()));

        TutorialConsole.WriteExperiment(
            3,
            "Application service orquestra domínio, queries e persistência",
            "Execute o mesmo caso de uso com email ausente inválido, email presente e duplicidade de email.");
        TutorialConsole.WriteCodeSnippet(
            "Application service funcional",
            typeof(RegisterUserService),
            nameof(RegisterUserService.RegisterAsync));
        TutorialConsole.WriteCodeSnippet(
            "ConfigurationMappings | ComplexProperty + HasConversion",
            typeof(UserAccountConfiguration),
            nameof(UserAccountConfiguration.Configure));

        await using var dbContext = CreateDbContext();

        TutorialConsole.WriteEvidence(
            "Mapeamento EF Core 10",
            ("Entity", dbContext.Model.FindEntityType(typeof(UserAccount))?.ClrType.Name ?? "não mapeada"),
            ("ComplexProperties", FormatComplexProperties(dbContext)));

        var service = new RegisterUserService(
            new EfUserAccountLookup(dbContext),
            new EfUserAccountWriter(dbContext),
            dbContext,
            TimeProvider.System);
        var missingEmail = await service.RegisterAsync(
            new RegisterUserRequest("Ada Lovelace", null, "11999998888"),
            cancellationToken).ConfigureAwait(false);
        var withEmail = await service.RegisterAsync(
            new RegisterUserRequest("Grace Hopper", "grace@example.com", null),
            cancellationToken).ConfigureAwait(false);
        var duplicateEmail = await service.RegisterAsync(
            new RegisterUserRequest("Outra Grace", "grace@example.com", null),
            cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Persistência assíncrona",
            ("Email ausente", ToScenarioResult(missingEmail)),
            ("Com email", ToScenarioResult(withEmail)),
            ("Email duplicado", ToScenarioResult(duplicateEmail)),
            ("Linhas persistidas", (await dbContext.Users.CountAsync(cancellationToken).ConfigureAwait(false)).ToString()));

        TutorialConsole.WriteExperiment(
            4,
            "HTTP fica como tradução explícita",
            "Mapeie Either<Seq<DomainError>, RegisteredUserDto> para Created, BadRequest ou Conflict sem interpretar null.");
        TutorialConsole.WriteCodeSnippet(
            "Endpoint mapper",
            typeof(RegistrationEndpoint),
            nameof(RegistrationEndpoint.ToHttpResult));

        var conflict = RegistrationEndpoint.ToHttpResult(duplicateEmail);
        TutorialConsole.WriteEvidence(
            "Retorno HTTP",
            ("Status duplicidade", GetStatusCode(conflict).ToString()),
            ("Regra esperada", "Conflict em vez de exception"));

        TutorialConsole.WriteConclusion(
            "O domínio não carrega propriedades nullable. Ausência válida usa Option<T>; regra esperada usa Either; falha técnica fica em Try/exception na borda.",
            TutorialConclusionKind.Success);
    }

    /// <summary>
    /// Creates an isolated SQLite in-memory context for the tutorial run.
    /// </summary>
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

    /// <summary>
    /// Formats an Either result for compact console evidence.
    /// </summary>
    private static string ToScenarioResult(LanguageExt.Either<LanguageExt.Seq<DomainError>, RegisteredUserDto> result)
    {
        return result.Match(
            Right: user => $"Right({user.Name})",
            Left: errors => $"Left({string.Join(", ", errors.Map(error => error.Code.ToString()))})");
    }

    /// <summary>
    /// Creates one aggregate directly to show that the domain raises an event without infrastructure.
    /// </summary>
    private static string CreateDomainEventEvidence()
    {
        var account =
            UserAccount.Create("Alan Turing", "alan@example.com", null, TimeProvider.System);

        return account.Match(
            Right: user =>
            {
                var events = user.DomainEvents.ToArray();
                return $"{events.Length} {events[0].GetType().Name}";
            },
            Left: errors => string.Join(", ", errors.Map(error => error.Code.ToString())));
    }

    /// <summary>
    /// Formats mapped complex properties so the tutorial proves EF is not using a persistence-only record.
    /// </summary>
    private static string FormatComplexProperties(RegistrationDbContext dbContext)
    {
        var userEntity = dbContext.Model.FindEntityType(typeof(UserAccount));

        return userEntity is null
            ? "nenhuma"
            : string.Join(", ", userEntity.GetComplexProperties().Select(property => property.Name));
    }

    /// <summary>
    /// Reads the status code exposed by ASP.NET Core result implementations.
    /// </summary>
    private static int GetStatusCode(IResult result)
    {
        return result is IStatusCodeHttpResult statusCodeResult
            ? statusCodeResult.StatusCode ?? StatusCodes.Status200OK
            : StatusCodes.Status200OK;
    }
}
