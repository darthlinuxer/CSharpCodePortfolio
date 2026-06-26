using System.Globalization;
using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial08.Application;
using EFCore10.Tutorials.Tutorial08.Domain;
using EFCore10.Tutorials.Tutorial08.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08;

[Tutorial("08", "relationship-mapping", "DDD universitario e relacionamentos")]
public sealed class RelationshipMappingTutorial : ITutorial
{
    private const string ConnectionStringName = "TutorialDatabase";

    /// <summary>
    /// Runs the university DDD relationship mapping tutorial.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(RelationshipMappingTutorial).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        TutorialConsole.WriteHeader("08", "DDD universitario e relacionamentos");
        TutorialConsole.WriteContext(
            ("Provider", "SQLite"),
            ("Database", SqliteConnectionStrings.GetDisplayDataSource(connectionString, AppContext.BaseDirectory)),
            ("Foco", "Aggregates, value objects, owned types e HasOne/HasMany"),
            ("Heranca", "Somente Employee -> Professor/AdministrativeEmployee em TPH"));
        TutorialConsole.WriteQuestion(
            "Como um dominio universitario rico fica persistido quando aggregate root nao e sinonimo de uma unica tabela?");
        TutorialConsole.WriteHypothesis(
            "Aggregates definem consistencia; o banco ainda pode ter tabelas para entidades internas e joins.",
            "Value objects deixam o dominio sem primitive obsession e sao salvos por converters.",
            "OwnsOne/OwnsMany aparecem quando o objeto nao vive sem o owner; HasOne/HasMany fica para identidade propria.");
        TutorialConsole.WritePreparation(
            "Leia cada configuracao como uma pergunta de modelagem: quem tem identidade, quem tem ciclo de vida proprio, qual FK pertence ao banco e qual regra pertence ao dominio?",
            "O metodo socratico usado aqui evita decorar Fluent API: cada comando responde uma duvida sobre identidade, conversao, relacionamento, delecao ou consulta.");
        TutorialConsole.WriteCodeSnippet(
            "Roteiro socratico para ler as classes em Persistence/Configurations.",
            "Configurations.socratic.md",
            MappingSocraticGuide);

        var domainRuleCount = ValidateDomainRules();
        TutorialConsole.WriteEvidence(
            "Self-checks de dominio",
            ("Regras exercitadas", $"{domainRuleCount} entradas invalidas rejeitadas com DomainException"),
            ("Limite academico", "Student bloqueia mais de 40 pontos no mesmo semestre"),
            ("Demissao", "University bloqueia professor ainda atribuido a curso ativo"));

        var options = new DbContextOptionsBuilder<UniversityContext>()
            .UseSqlite(connectionString)
            .Options;

        await using var context = new UniversityContext(options);
        var applicationService = new UniversityApplicationService(context);
        await applicationService.RecreateSampleAsync(cancellationToken).ConfigureAwait(false);

        var semester = Semester.Create(2026, 1);
        var dashboardReadService = new UniversityDashboardReadService(context);
        var courseAnalyticsReadService = new CourseAnalyticsReadService(context);
        var studentProgressReadService = new StudentProgressReadService(context);
        var facultyWorkloadReadService = new FacultyWorkloadReadService(context);

        var dashboardSql = dashboardReadService.GetUniversityDashboardSql();
        var courseAnalyticsSql = courseAnalyticsReadService.GetCourseAnalyticsSql();
        var studentProgressSql = studentProgressReadService.GetStudentProgressSql(semester);
        var facultyWorkloadSql = facultyWorkloadReadService.GetFacultyWorkloadSql();
        var dashboard = await dashboardReadService.GetUniversityDashboardAsync(cancellationToken).ConfigureAwait(false);
        var courseAnalytics = await courseAnalyticsReadService.GetCourseAnalyticsAsync(cancellationToken).ConfigureAwait(false);
        var studentProgress = await studentProgressReadService
            .GetStudentProgressAsync(semester, cancellationToken)
            .ConfigureAwait(false);
        var facultyWorkloads = await facultyWorkloadReadService.GetFacultyWorkloadAsync(cancellationToken).ConfigureAwait(false);
        ValidateReadModels(dashboard, courseAnalytics, studentProgress, facultyWorkloads);

        var schema = await ReadSchemaAsync(context, cancellationToken).ConfigureAwait(false);
        var tableNames = schema
            .Where(static item => string.Equals(item.Type, "table", StringComparison.OrdinalIgnoreCase))
            .Select(static item => item.Name)
            .ToArray();
        var columns = await ReadColumnsAsync(context, tableNames, cancellationToken).ConfigureAwait(false);
        var schemaConclusion = ValidateSchema(schema, columns);

        TutorialConsole.WriteCodeSnippet(
            "Consulta DTO de dashboard gerada pelo read service.",
            "UniversityDashboard.sql",
            dashboardSql);
        TutorialConsole.WriteCodeSnippet(
            "Consulta de demanda, professor e media por curso.",
            "CourseAnalytics.sql",
            courseAnalyticsSql);
        TutorialConsole.WriteCodeSnippet(
            "Consulta de progresso discente por semestre.",
            "StudentProgress.sql",
            studentProgressSql);
        TutorialConsole.WriteCodeSnippet(
            "Consulta de carga docente por curso.",
            "FacultyWorkload.sql",
            facultyWorkloadSql);
        WriteMappingMetadata(context);
        TutorialConsole.WriteCodeSnippet(
            "Schema SQLite criado para o modelo DDD, incluindo indices.",
            "sqlite_master",
            FormatSchema(schema));
        TutorialConsole.WriteEvidence(
            "Modelo observado",
            ("Tabelas", string.Join(", ", tableNames)),
            ("Dashboard", $"{dashboard.UniversityName}; {dashboard.DepartmentCount} departamento(s); {dashboard.CourseCount} curso(s); {dashboard.StudentCount} aluno(s)"),
            ("Funcionarios", $"{dashboard.EmployeeCount} total; {dashboard.ProfessorCount} professor(es); {dashboard.AdministrativeEmployeeCount} administrativo(s); {dashboard.ActiveEmployeeCount} ativo(s)"),
            ("Cursos", string.Join(", ", courseAnalytics.Select(course => $"{course.CourseCode} {course.CreditPoints} pts"))),
            ("Demanda", string.Join(", ", courseAnalytics.Select(course => $"{course.CourseCode}: {course.EnrollmentCount} matricula(s), media {FormatNullableGrade(course.AverageFinalGrade)}"))),
            ("Carga discente", string.Join(", ", studentProgress.Select(load => $"{load.StudentName}: {load.TotalCreditPoints} pts ({load.RemainingCreditPoints} restantes), media {FormatNullableGrade(load.AverageFinalGrade)}"))),
            ("Carga docente", string.Join(", ", facultyWorkloads.Select(workload => $"{workload.ProfessorName}: {workload.CourseCount} curso(s), {workload.TotalCreditPoints} pts, {workload.EnrollmentCount} matricula(s)"))),
            ("OwnsOne", courseAnalytics.Single(course => course.CourseCode == "CS-EF-101").SyllabusSummary),
            ("Validacao do schema", schemaConclusion));
        TutorialConsole.WriteConclusion(
            "O dominio e salvo por aggregate roots, mas o schema relacional continua tendo entidades internas, owned tables e join entities.",
            TutorialConclusionKind.Success);
        TutorialConsole.WriteCleanup(
            "O banco foi recriado e mantido no diretorio de saida para inspecao local.");
    }

    private const string MappingSocraticGuide = """
        # Roteiro socratico das configuracoes

        ## Perguntas-base

        1. Este tipo e aggregate root, entidade interna, owned type, join entity ou subtipo de uma hierarquia?
        2. Ele precisa de tabela propria, coluna no owner ou discriminador?
        3. O dominio precisa enxergar o ID/FK, ou isso e so detalhe relacional?
        4. A colecao deve passar pelo backing field para preservar invariantes?
        5. A exclusao deve cascatear ou ser bloqueada para proteger consistencia?
        6. Qual consulta do tutorial justifica cada indice?

        ## Comandos e razoes

        - ToTable: fixa o nome fisico da tabela para o schema ensinar a linguagem do dominio.
        - HasTableMapping: anotacao didatica do tutorial; nao muda o EF, mas permite explicar TPH, concrete table, internal entity, OwnsOne, OwnsMany e join entity na saida.
        - UseTphMappingStrategy, HasDiscriminator e HasValue: guardam Professor e AdministrativeEmployee em Employees com EmployeeType, porque Tutorial08 so precisa de uma hierarquia curta.
        - Navigation(...).UsePropertyAccessMode(Field): faz o EF preencher backing fields privados sem abrir List<T> mutavel no dominio.
        - Ignore: impede que propriedades de conveniencia, como Courses ou Students, virem outro relacionamento duplicado.
        - HasKey: declara a identidade relacional que corresponde a identidade do dominio, ou a chave composta da join entity.
        - Property: mostra explicitamente cada coluna que sai de value object, shadow FK ou payload.
        - HasConversion: transforma value object em escalar persistivel e reconstrui o tipo validado ao materializar.
        - ValueGeneratedNever: diz que o dominio cria o ID; o banco nao substitui a identidade do aggregate.
        - HasMaxLength e IsRequired: levam invariantes simples para o schema sem retirar a validacao do dominio.
        - Property<T>(Columns.*): cria shadow FK quando o banco precisa da coluna, mas o dominio ja navega por objeto.
        - HasColumnName: nomeia colunas owned para mostrar que Syllabus vive em Courses.
        - OwnsOne: Syllabus nao tem identidade fora de Course, entao vira colunas do owner.
        - OwnsMany, WithOwner e HasForeignKey: Campus pertence a University; a colecao precisa de tabela, mas nao de repository proprio.
        - HasOne, WithMany e HasForeignKey: modelam associacoes entre entidades com identidade propria.
        - OnDelete(Cascade): remove dependentes que nao fazem sentido sem o owner, como Campus ou Enrollment.
        - OnDelete(Restrict): bloqueia exclusoes que poderiam quebrar referencias importantes, como Employee, Department e Professor.
        - HasIndex, IsUnique e HasDatabaseName: deixam visiveis os acessos usados pelas read queries e documentam unicidade esperada pelo dominio.
        - HasPrecision: preserva a forma numerica de Grade sem espalhar decimal cru no dominio.
        """;

    private static int ValidateDomainRules()
    {
        var checks = new (string Scenario, string ExpectedCode, Action Action)[]
        {
            ("nome nulo", DomainErrors.RequiredText, static () => _ = new University(UniversityName.Create(null))),
            ("email invalido", DomainErrors.EmailInvalid, static () => _ = new Student(PersonName.Create("Ana Valida"), EmailAddress.Create("not email"))),
            ("data local", DomainErrors.UtcRequired, static () => _ = UtcDateTime.Create(DateTime.Now)),
            ("nota fora da faixa", DomainErrors.GradeInvalid, static () => _ = Grade.Create(11m)),
            ("campus duplicado", DomainErrors.CampusNameDuplicated, static () =>
            {
                var university = new University(UniversityName.Create("Universidade Valida"));
                university.AddCampus(CampusName.Create("Main"), CityName.Create("Sao Paulo"));
                university.AddCampus(CampusName.Create("Main"), CityName.Create("Campinas"));
            }),
            ("departamento de outra universidade", DomainErrors.ProfessorDepartmentMismatch, static () =>
            {
                var first = new University(UniversityName.Create("Primeira Universidade"));
                var second = new University(UniversityName.Create("Segunda Universidade"));
                var department = first.OpenDepartment(DepartmentName.Create("Computer Science"));

                second.HireProfessor(
                    PersonName.Create("Professora Valida"),
                    EmailAddress.Create("professora.valida@contoso.edu"),
                    department,
                    UtcDateTime.Create(DateTime.UtcNow));
            }),
            ("limite de pontos no semestre", DomainErrors.StudentCreditLimitExceeded, static () =>
            {
                var student = new Student(PersonName.Create("Aluno Valido"), EmailAddress.Create("aluno.valido@contoso.edu"));
                var semester = Semester.Create(2026, 1);
                var enrolledAtUtc = UtcDateTime.Create(DateTime.UtcNow);

                student.RegisterForCourse(CreateValidCourse("Curso Valido A", "CS-301", 25), semester, enrolledAtUtc);
                student.RegisterForCourse(CreateValidCourse("Curso Valido B", "CS-302", 20), semester, enrolledAtUtc);
            }),
            ("professor com curso ativo nao pode ser demitido", DomainErrors.EmployeeDismissalBlocked, static () =>
            {
                var university = new University(UniversityName.Create("Universidade Valida"));
                var department = university.OpenDepartment(DepartmentName.Create("Computer Science"));
                var hiredAtUtc = UtcDateTime.Create(DateTime.UtcNow);
                var professor = university.HireProfessor(
                    PersonName.Create("Professora Valida"),
                    EmailAddress.Create("professora.valida@contoso.edu"),
                    department,
                    hiredAtUtc);
                var course = CreateValidCourse("Curso Valido", "CS-401", 10);
                course.AssignProfessor(professor);

                university.DismissEmployee(professor, UtcDateTime.Create(hiredAtUtc.Value.AddDays(1)), [course]);
            })
        };

        foreach (var (scenario, expectedCode, action) in checks)
        {
            ExpectDomainException(action, scenario, expectedCode);
        }

        return checks.Length;
    }

    private static void ValidateReadModels(
        UniversityDashboardDto dashboard,
        IReadOnlyList<CourseAnalyticsDto> courseAnalytics,
        IReadOnlyList<StudentProgressDto> studentProgress,
        IReadOnlyList<FacultyWorkloadDto> facultyWorkloads)
    {
        if (dashboard is not
            {
                UniversityName: "Contoso University",
                DepartmentCount: 2,
                CourseCount: 4,
                StudentCount: 3,
                EnrollmentCount: 7,
                ProfessorCount: 2,
                AdministrativeEmployeeCount: 1
            })
        {
            throw new InvalidOperationException("Dashboard DTO did not match the expected sample state.");
        }

        var relationshipMapping = courseAnalytics.Single(course => course.CourseCode == "CS-EF-101");
        if (relationshipMapping.ProfessorName != "Grace Hopper"
            || relationshipMapping.DepartmentName != "Computer Science"
            || relationshipMapping.EnrollmentCount != 2
            || relationshipMapping.AverageFinalGrade != 8.75m)
        {
            throw new InvalidOperationException("Course analytics DTO did not include the expected professor, demand and average grade.");
        }

        var anaProgress = studentProgress.Single(load => load.StudentName == "Ana Pereira");
        if (anaProgress.TotalCreditPoints != 35 || anaProgress.RemainingCreditPoints != 5 || anaProgress.AverageFinalGrade != 9.15m)
            throw new InvalidOperationException("Student progress DTO did not preserve the expected semester credit total and average.");

        var graceWorkload = facultyWorkloads.Single(workload => workload.ProfessorName == "Grace Hopper");
        if (graceWorkload.CourseCount != 2 || graceWorkload.TotalCreditPoints != 35 || graceWorkload.EnrollmentCount != 4)
            throw new InvalidOperationException("Faculty workload DTO did not include the expected Grace Hopper assignment.");
    }

    private static string FormatNullableGrade(decimal? grade) =>
        grade.HasValue ? grade.Value.ToString("0.00", CultureInfo.InvariantCulture) : "sem nota";

    private static Course CreateValidCourse(string title, string code, int points) =>
        new(
            CourseTitle.Create(title),
            CourseCode.Create(code),
            CreditPoints.Create(points),
            new Syllabus(
                SyllabusSummary.Create("Resumo valido para o curso."),
                SyllabusOutcomes.Create("Resultados validos para o curso.")));

    private static void ExpectDomainException(Action action, string scenario, string expectedCode)
    {
        try
        {
            action();
        }
        catch (DomainException exception) when (string.Equals(exception.Code, expectedCode, StringComparison.Ordinal))
        {
            return;
        }
        catch (DomainException exception)
        {
            throw new InvalidOperationException(
                $"{scenario} raised DomainException code {exception.Code} instead of {expectedCode}.",
                exception);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"{scenario} raised {exception.GetType().Name} instead of DomainException.",
                exception);
        }

        throw new InvalidOperationException($"{scenario} did not raise DomainException.");
    }

    private static void WriteMappingMetadata(UniversityContext context)
    {
        var mappings = context.Model.GetEntityTypes()
            .Select(static entityType =>
            {
                var table = entityType.GetTableName();
                var mapping = entityType.FindAnnotation(TableMappingMetadata.AnnotationName)?.Value?.ToString();
                var entity = entityType.ClrType == typeof(Dictionary<string, object>)
                    ? table
                    : entityType.ClrType.Name;
                var target = string.IsNullOrWhiteSpace(table)
                    ? $"{entity} (sem tabela propria)"
                    : string.Equals(table, entity, StringComparison.Ordinal)
                    ? table
                    : $"{table}/{entity}";

                return (Mapping: mapping, Target: target);
            })
            .Where(static item => !string.IsNullOrWhiteSpace(item.Target) && !string.IsNullOrWhiteSpace(item.Mapping))
            .ToArray();

        TutorialConsole.WriteEvidence(
            "Estrategia de mapeamento EF",
            ("TPH", FormatMappingTargets(mappings, TableMappingMetadata.Tph)),
            ("Tabelas concretas", FormatMappingTargets(mappings, TableMappingMetadata.ConcreteTable)),
            ("Entidades internas", FormatMappingTargets(mappings, TableMappingMetadata.InternalEntity)),
            ("OwnsOne", FormatMappingTargets(mappings, TableMappingMetadata.OwnsOne)),
            ("OwnsMany", FormatMappingTargets(mappings, TableMappingMetadata.OwnsMany)),
            ("Join entities", FormatMappingTargets(mappings, TableMappingMetadata.JoinEntity)));
    }

    private static string FormatMappingTargets(
        IEnumerable<(string? Mapping, string Target)> mappings,
        string mapping)
    {
        var targets = mappings
            .Where(item => string.Equals(item.Mapping, mapping, StringComparison.Ordinal))
            .Select(static item => item.Target!)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return targets is [] ? "nenhum" : string.Join(", ", targets);
    }

    private static async Task<IReadOnlyList<SqliteSchemaObject>> ReadSchemaAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        var schema = new List<SqliteSchemaObject>();
        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                SELECT type, name, sql
                FROM sqlite_master
                WHERE type IN ('table', 'index') AND name NOT LIKE 'sqlite_%' AND sql IS NOT NULL
                ORDER BY CASE type WHEN 'table' THEN 0 ELSE 1 END, name;
                """;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                schema.Add(new SqliteSchemaObject(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
            }

            return schema;
        }
        finally
        {
            await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyDictionary<string, IReadOnlySet<string>>> ReadColumnsAsync(
        DbContext context,
        IEnumerable<string> tableNames,
        CancellationToken cancellationToken)
    {
        var tables = new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase);
        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            foreach (var tableName in tableNames)
            {
                var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                await using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = $"PRAGMA table_info(\"{tableName.Replace("\"", "\"\"", StringComparison.Ordinal)}\");";

                await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    columns.Add(reader.GetString(1));
                }

                tables[tableName] = columns;
            }

            return tables;
        }
        finally
        {
            await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        }
    }

    private static string ValidateSchema(
        IReadOnlyCollection<SqliteSchemaObject> schema,
        IReadOnlyDictionary<string, IReadOnlySet<string>> columns)
    {
        var tables = GetTableNames(schema);
        EnsureTables(
            tables,
            required: [Tables.Universities, Tables.UniversityCampuses, Tables.Departments, Tables.Employees, Tables.Courses, Tables.Students, Tables.Enrollments],
            forbidden: ["People", "Users", "Teachers", "StaffEmployees"]);
        EnsureNoLegacyTables(tables);
        EnsureColumns(columns, Tables.Employees, ["Id", "EmployeeType", "Name", "Email", Columns.UniversityId, Columns.DepartmentId, "Role", "Status", "HiredAtUtc", "DismissedAtUtc"]);
        EnsureColumns(columns, Tables.Courses, ["Id", "Title", "Code", "CreditPoints", Columns.ProfessorId, "SyllabusSummary", "SyllabusOutcomes"]);
        EnsureColumns(columns, Tables.UniversityCampuses, [Columns.UniversityId, "Id", "Name", "City"]);
        EnsureColumns(columns, Tables.Enrollments, [Columns.StudentId, Columns.CourseId, "Semester", "EnrolledAtUtc", "FinalGrade"]);
        EnsureColumns(columns, Tables.Departments, [Columns.UniversityId, "Name"]);
        EnsureColumns(columns, Tables.Students, ["Id", "Name", "Email"]);

        return "schema reflete aggregate roots, entidade interna, TPH, OwnsOne, OwnsMany e join entity";
    }

    private static IReadOnlySet<string> GetTableNames(IEnumerable<SqliteSchemaObject> schema) =>
        schema
            .Where(static item => string.Equals(item.Type, "table", StringComparison.OrdinalIgnoreCase))
            .Select(static item => item.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static void EnsureTables(
        IReadOnlySet<string> tables,
        IReadOnlyCollection<string> required,
        IReadOnlyCollection<string> forbidden)
    {
        var missing = required.Where(table => !tables.Contains(table)).ToArray();
        var unexpected = forbidden.Where(tables.Contains).ToArray();

        if (missing is [] && unexpected is [])
            return;

        throw new InvalidOperationException(
            $"Schema mismatch. Missing: {FormatList(missing)}. Unexpected: {FormatList(unexpected)}.");
    }

    private static void EnsureNoLegacyTables(IReadOnlySet<string> tables)
    {
        var legacyTables = new[]
            {
                "Customers",
                "CustomerContactMethods",
                "Orders",
                "OrderItems",
                "Products",
                "Categories",
                "ProductCategories",
                "AuditLogs",
                "Roles",
                "UserRoles"
            }
            .Where(tables.Contains)
            .ToArray();

        if (legacyTables is not [])
            throw new InvalidOperationException($"Legacy tables still exist: {string.Join(", ", legacyTables)}.");
    }

    private static void EnsureColumns(
        IReadOnlyDictionary<string, IReadOnlySet<string>> tables,
        string tableName,
        IReadOnlyCollection<string> expectedColumns)
    {
        if (!tables.TryGetValue(tableName, out var actualColumns))
            throw new InvalidOperationException($"Table {tableName} was not created.");

        var missingColumns = expectedColumns
            .Where(expectedColumn => !actualColumns.Contains(expectedColumn))
            .ToArray();

        if (missingColumns is [])
            return;

        throw new InvalidOperationException(
            $"{tableName} is missing columns: {string.Join(", ", missingColumns)}.");
    }

    private static string FormatSchema(IEnumerable<SqliteSchemaObject> schema) =>
        string.Join(
            $"{Environment.NewLine}{Environment.NewLine}",
            schema.Select(item => $"-- {item.Type}: {item.Name}{Environment.NewLine}{item.Sql};"));

    private static string FormatList(IReadOnlyCollection<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);

    private sealed record UniversitySample(University University, Course[] Courses, Student[] Students);
}
