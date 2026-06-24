using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpCodePortfolio.Tutorials.Tutorial13;

[Tutorial("13", "efcore-inmemory-services", "EF Core InMemory usando Services")]
public sealed class EfCoreInMemoryServicesTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("13", "EF Core InMemory usando Services");
        TutorialConsole.WriteContext(
            ("Tema", "EF Core InMemory com DI"),
            ("Conceito", "O DbContext é configurado no container e consumido por serviços de aplicação"),
            ("Provider", "Microsoft.EntityFrameworkCore.InMemory"),
            ("Runtime", ".NET 10"),
            ("Slug", "efcore-inmemory-services"));
        TutorialConsole.WriteQuestion("Como usar EF Core InMemory com `IServiceCollection` e serviços de aplicação?");
        TutorialConsole.WriteHypothesis(
            "`AddDbContext` registra o contexto com ciclo de vida scoped.",
            "Um serviço de aplicação recebe o `DbContext` por construtor.",
            "Escopos diferentes podem trabalhar sobre o mesmo banco em memória nomeado.");
        TutorialConsole.WritePreparation(
            "O container registra `SchoolDbContext` e `SchoolService`.",
            "Um escopo cadastra estudantes, professor e curso.",
            "Outro escopo consulta o relatório final usando o mesmo banco em memória.");

        var databaseName = $"dbSchool-services-{Guid.NewGuid():N}";

        TutorialConsole.WriteExperiment(
            1,
            "Registro dos serviços",
            "Configura EF Core InMemory no container e registra o serviço de aplicação.");
        TutorialConsole.WriteCodeSnippet(
            "O DbContext é resolvido pelo container; o código de uso resolve `SchoolService`.",
            typeof(SchoolServiceRegistration),
            nameof(SchoolServiceRegistration.Build),
            new CodeExcerpt(5, 7, "Registro do DbContext e serviço"));

        TutorialConsole.WriteExperiment(
            2,
            "Execução por escopos",
            "Cria um escopo para gravação e outro para leitura, preservando o ciclo de vida scoped.");
        TutorialConsole.WriteCodeSnippet(
            "Cada escopo recebe uma instância própria do serviço e do contexto.",
            typeof(EfCoreInMemoryServicesTutorial),
            nameof(RunAsync),
            new CodeExcerpt(42, 55, "Escopos de escrita e leitura"));

        using var serviceProvider = SchoolServiceRegistration.Build(databaseName);

        using (var writeScope = serviceProvider.CreateScope())
        {
            var writer = writeScope.ServiceProvider.GetRequiredService<SchoolService>();
            await writer.CreateSchoolAsync(cancellationToken);
        }

        SchoolReport report;
        using (var readScope = serviceProvider.CreateScope())
        {
            var reader = readScope.ServiceProvider.GetRequiredService<SchoolService>();
            report = await reader.BuildReportAsync(cancellationToken);
        }

        VerifyReport(report);

        TutorialConsole.WriteEvidence(
            "Serviços EF Core",
            ("DbContext", "registrado via AddDbContext"),
            ("Serviço", "SchoolService resolvido por escopo"),
            ("Estudantes", string.Join(" | ", report.Students)),
            ("Professores", string.Join(" | ", report.Teachers)),
            ("Curso", report.Course),
            ("Matriculados", string.Join(" | ", report.EnrolledStudents)),
            ("Total de estudantes", report.StudentCount.ToString()));

        TutorialConsole.WriteObservation(
            "A regra de uso fica no serviço de aplicação; o tutorial ainda mostra o EF Core, mas sem espalhar `DbContext` pelo fluxo principal.");
        TutorialConsole.WriteConclusion(
            "`AddDbContext` mais serviços scoped é a composição mínima para ensinar EF Core InMemory com injeção de dependência.",
            TutorialConclusionKind.Success);
    }

    private static void VerifyReport(SchoolReport report)
    {
        if (report.StudentCount != 2)
        {
            throw new InvalidOperationException("O banco em memória deve conter dois estudantes.");
        }

        if (!report.Course.Contains("How to be a Rock Star", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("O curso esperado não foi consultado.");
        }

        if (report.EnrolledStudents.Count != 2)
        {
            throw new InvalidOperationException("O curso deve possuir dois estudantes matriculados.");
        }
    }
}
