using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial12;

[Tutorial("12", "efcore-inmemory-direct", "EF Core InMemory sem IRepository")]
public sealed class EfCoreInMemoryDirectTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("12", "EF Core InMemory sem IRepository");
        TutorialConsole.WriteContext(
            ("Tema", "EF Core InMemory"),
            ("Conceito", "O DbContext pode ser usado diretamente quando o tutorial quer ensinar EF Core"),
            ("Provider", "Microsoft.EntityFrameworkCore.InMemory"),
            ("Runtime", ".NET 10"),
            ("Slug", "efcore-inmemory-direct"));
        TutorialConsole.WriteQuestion("Como usar EF Core InMemory diretamente, sem criar uma camada `IRepository`?");
        TutorialConsole.WriteHypothesis(
            "O `DbContext` já expõe unidades de trabalho e conjuntos de entidades.",
            "Um banco em memória nomeado pode ser compartilhado por mais de uma instância do contexto.",
            "Relacionamentos podem ser persistidos e consultados com `Include` sem repositório intermediário.");
        TutorialConsole.WritePreparation(
            "O exemplo cadastra estudantes, professor e curso.",
            "O curso recebe o professor e os estudantes matriculados.",
            "Uma segunda instância do contexto consulta os dados gravados no mesmo banco em memória.");

        var databaseName = $"dbSchool-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<SchoolDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        TutorialConsole.WriteExperiment(
            1,
            "Configuração do provider",
            "Cria as opções do contexto com um banco em memória nomeado.");
        TutorialConsole.WriteCodeSnippet(
            "O nome do banco conecta as instâncias do contexto durante a execução do tutorial.",
            "DbContextOptions.cs",
            """
            var options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "dbSchool")
                .Options;
            """);

        TutorialConsole.WriteExperiment(
            2,
            "Uso direto do DbContext",
            "Grava entidades e consulta relacionamentos sem criar um repositório intermediário.");
        TutorialConsole.WriteCodeSnippet(
            "O fluxo usa `DbSet`, `SaveChangesAsync` e `Include` diretamente.",
            "SchoolScenario.cs",
            """
            await context.Students.AddRangeAsync([camilo, aline], cancellationToken);
            await context.Teachers.AddAsync(teacher, cancellationToken);
            await context.Courses.AddAsync(course, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var savedCourse = await context.Courses
                .Include(course => course.Teacher)
                .Include(course => course.Students)
                .SingleAsync(cancellationToken);
            """);

        var report = await new SchoolScenario(options).RunAsync(cancellationToken);
        VerifyReport(report);

        TutorialConsole.WriteEvidence(
            "Banco em memória",
            ("Estudantes", string.Join(" | ", report.Students)),
            ("Professores", string.Join(" | ", report.Teachers)),
            ("Curso", report.Course),
            ("Matriculados", string.Join(" | ", report.EnrolledStudents)),
            ("Total de estudantes", report.StudentCount.ToString()));

        TutorialConsole.WriteObservation(
            "Sem `IRepository`, o tutorial deixa a API do EF Core visível: `DbSet`, rastreamento, `SaveChangesAsync` e carregamento de navegações.");
        TutorialConsole.WriteConclusion(
            "EF Core InMemory é suficiente para demonstrar modelagem, relacionamento e consultas em memória quando a intenção é ensinar o próprio DbContext.",
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
