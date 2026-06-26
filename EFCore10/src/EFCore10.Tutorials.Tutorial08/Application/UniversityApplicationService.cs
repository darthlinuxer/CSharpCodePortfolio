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
        var hiredAtUtc = UtcDateTime.Create(DateTime.SpecifyKind(new DateTime(2026, 6, 25, 14, 0, 0), DateTimeKind.Utc));
        var enrolledAtUtc = UtcDateTime.Create(hiredAtUtc.Value.AddHours(2));
        var semester = Semester.Create(2026, 1);

        var university = new University(UniversityName.Create("Contoso University"));
        university.AddCampus(CampusName.Create("Main Campus"), CityName.Create("Sao Paulo"));
        university.AddCampus(CampusName.Create("Research Campus"), CityName.Create("Campinas"));

        var computerScience = university.OpenDepartment(DepartmentName.Create("Computer Science"));
        var dataScience = university.OpenDepartment(DepartmentName.Create("Data Science"));
        var grace = university.HireProfessor(
            PersonName.Create("Grace Hopper"),
            EmailAddress.Create("grace.hopper@contoso.edu"),
            computerScience,
            hiredAtUtc);
        var katherine = university.HireProfessor(
            PersonName.Create("Katherine Johnson"),
            EmailAddress.Create("katherine.johnson@contoso.edu"),
            dataScience,
            hiredAtUtc);
        university.HireAdministrativeEmployee(
            PersonName.Create("Alan Turing"),
            EmailAddress.Create("alan.turing@contoso.edu"),
            StaffRole.Create("Registrar"),
            hiredAtUtc);

        var relationshipMapping = new Course(
            computerScience,
            CourseTitle.Create("EF Core Relationship Mapping"),
            CourseCode.Create("CS-EF-101"),
            CreditPoints.Create(20),
            new Syllabus(
                SyllabusSummary.Create("Modelar relacoes universitarias com EF Core."),
                SyllabusOutcomes.Create("Distinguir aggregate root, entidade interna, OwnsOne, OwnsMany, HasOne e HasMany.")));
        relationshipMapping.AssignProfessor(grace);

        var domainModeling = new Course(
            computerScience,
            CourseTitle.Create("DDD Tactical Modeling"),
            CourseCode.Create("CS-DDD-201"),
            CreditPoints.Create(15),
            new Syllabus(
                SyllabusSummary.Create("Modelar invariantes e boundaries de aggregates."),
                SyllabusOutcomes.Create("Aplicar value objects, entidades internas e regras no dominio.")));
        domainModeling.AssignProfessor(grace);

        var queryOptimization = new Course(
            dataScience,
            CourseTitle.Create("EF Core Query Optimization"),
            CourseCode.Create("CS-EF-202"),
            CreditPoints.Create(10),
            new Syllabus(
                SyllabusSummary.Create("Projetar read models eficientes para relatorios academicos."),
                SyllabusOutcomes.Create("Aplicar Select, AsNoTracking, agregacoes e indices em consultas EF Core.")));
        queryOptimization.AssignProfessor(katherine);

        var reporting = new Course(
            dataScience,
            CourseTitle.Create("Campus Operations Reporting"),
            CourseCode.Create("CS-OPS-105"),
            CreditPoints.Create(5),
            new Syllabus(
                SyllabusSummary.Create("Transformar relacoes academicas em indicadores operacionais."),
                SyllabusOutcomes.Create("Ler dados relacionais sem expor entidades de dominio como contrato.")));

        var ana = new Student(PersonName.Create("Ana Pereira"), EmailAddress.Create("ana.pereira@contoso.edu"));
        var bia = new Student(PersonName.Create("Bia Santos"), EmailAddress.Create("bia.santos@contoso.edu"));
        var caio = new Student(PersonName.Create("Caio Lima"), EmailAddress.Create("caio.lima@contoso.edu"));
        ana.RegisterForCourse(relationshipMapping, semester, enrolledAtUtc).RecordFinalGrade(Grade.Create(9.5m));
        ana.RegisterForCourse(domainModeling, semester, enrolledAtUtc).RecordFinalGrade(Grade.Create(8.8m));
        bia.RegisterForCourse(relationshipMapping, semester, enrolledAtUtc).RecordFinalGrade(Grade.Create(8.0m));
        bia.RegisterForCourse(queryOptimization, semester, enrolledAtUtc).RecordFinalGrade(Grade.Create(9.2m));
        bia.RegisterForCourse(reporting, semester, enrolledAtUtc).RecordFinalGrade(Grade.Create(8.5m));
        caio.RegisterForCourse(domainModeling, semester, enrolledAtUtc).RecordFinalGrade(Grade.Create(7.6m));
        caio.RegisterForCourse(queryOptimization, semester, enrolledAtUtc).RecordFinalGrade(Grade.Create(9.0m));

        return new UniversitySample(
            university,
            [relationshipMapping, domainModeling, queryOptimization, reporting],
            [ana, bia, caio]);
    }

    private sealed record UniversitySample(University University, Course[] Courses, Student[] Students);
}
