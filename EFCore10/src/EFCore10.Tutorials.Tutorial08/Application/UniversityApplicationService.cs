using EFCore10.Tutorials.Tutorial08.Domain;
using EFCore10.Tutorials.Tutorial08.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Application;

internal sealed class UniversityApplicationService(UniversityContext context)
{
    /// <summary>
    /// Recreates the database and saves the sample university through domain behavior.
    /// </summary>
    public async Task RecreateSampleAsync(CancellationToken cancellationToken)
    {
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        var sample = CreateUniversitySample();
        context.Add(sample.University);
        context.AddRange(sample.Courses);
        context.AddRange(sample.Students);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
    }

    private static UniversitySample CreateUniversitySample()
    {
        var hiredAtUtc = DateTime.SpecifyKind(new DateTime(2026, 6, 25, 14, 0, 0), DateTimeKind.Utc);
        var enrolledAtUtc = hiredAtUtc.AddHours(2);

        var university = University.Create("Contoso University").RequireValue();
        university.AddCampus("Main Campus", "Sao Paulo").RequireSuccess();
        university.AddCampus("Research Campus", "Campinas").RequireSuccess();

        var computerScience = university.OpenDepartment("Computer Science").RequireValue();
        var dataScience = university.OpenDepartment("Data Science").RequireValue();
        var grace = university.HireProfessor(
            "Grace Hopper",
            "grace.hopper@contoso.edu",
            computerScience,
            hiredAtUtc).RequireValue();
        var katherine = university.HireProfessor(
            "Katherine Johnson",
            "katherine.johnson@contoso.edu",
            dataScience,
            hiredAtUtc).RequireValue();
        university.HireAdministrativeEmployee(
            "Alan Turing",
            "alan.turing@contoso.edu",
            "Registrar",
            hiredAtUtc).RequireValue();

        var relationshipMapping = Course.Create(
            computerScience,
            "EF Core Relationship Mapping",
            "CS-EF-101",
            20,
            "Modelar relacoes universitarias com EF Core.",
            "Distinguir aggregate root, entidade interna, OwnsOne, OwnsMany, HasOne e HasMany.").RequireValue();
        relationshipMapping.AssignProfessor(grace).RequireSuccess();

        var domainModeling = Course.Create(
            computerScience,
            "DDD Tactical Modeling",
            "CS-DDD-201",
            15,
            "Modelar invariantes e boundaries de aggregates.",
            "Aplicar value objects, entidades internas e regras no dominio.").RequireValue();
        domainModeling.AssignProfessor(grace).RequireSuccess();

        var queryOptimization = Course.Create(
            dataScience,
            "EF Core Query Optimization",
            "CS-EF-202",
            10,
            "Projetar read models eficientes para relatorios academicos.",
            "Aplicar Select, AsNoTracking, agregacoes e indices em consultas EF Core.").RequireValue();
        queryOptimization.AssignProfessor(katherine).RequireSuccess();

        var reporting = Course.Create(
            dataScience,
            "Campus Operations Reporting",
            "CS-OPS-105",
            5,
            "Transformar relacoes academicas em indicadores operacionais.",
            "Ler dados relacionais sem expor entidades de dominio como contrato.").RequireValue();

        var ana = Student.Create("Ana Pereira", "ana.pereira@contoso.edu").RequireValue();
        var bia = Student.Create("Bia Santos", "bia.santos@contoso.edu").RequireValue();
        var caio = Student.Create("Caio Lima", "caio.lima@contoso.edu").RequireValue();
        ana.RegisterForCourse(relationshipMapping, 2026, 1, enrolledAtUtc).RequireValue().RecordFinalGrade(9.5m).RequireSuccess();
        ana.RegisterForCourse(domainModeling, 2026, 1, enrolledAtUtc).RequireValue().RecordFinalGrade(8.8m).RequireSuccess();
        bia.RegisterForCourse(relationshipMapping, 2026, 1, enrolledAtUtc).RequireValue().RecordFinalGrade(8.0m).RequireSuccess();
        bia.RegisterForCourse(queryOptimization, 2026, 1, enrolledAtUtc).RequireValue().RecordFinalGrade(9.2m).RequireSuccess();
        bia.RegisterForCourse(reporting, 2026, 1, enrolledAtUtc).RequireValue().RecordFinalGrade(8.5m).RequireSuccess();
        caio.RegisterForCourse(domainModeling, 2026, 1, enrolledAtUtc).RequireValue().RecordFinalGrade(7.6m).RequireSuccess();
        caio.RegisterForCourse(queryOptimization, 2026, 1, enrolledAtUtc).RequireValue().RecordFinalGrade(9.0m).RequireSuccess();

        return new UniversitySample(
            university,
            [relationshipMapping, domainModeling, queryOptimization, reporting],
            [ana, bia, caio]);
    }

    private sealed record UniversitySample(University University, Course[] Courses, Student[] Students);
}
