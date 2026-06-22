using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;
using EFCore10.Tutorials.Tutorial07.Models;
using EFCore10.Tutorials.Tutorial07.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial07;

[Tutorial("07", "hierarchies", "Table per Hierarchy, Concrete Type, Type : TPH TPC TPT")]
public sealed class Tutorial07 : ITutorial
{
    private const string TphConnectionStringName = "TphDatabase";
    private const string TptConnectionStringName = "TptDatabase";
    private const string TpcConnectionStringName = "TpcDatabase";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(Tutorial07).Assembly);
        var tphConnectionString = GetConnectionString(tutorialConfiguration, TphConnectionStringName);
        var tptConnectionString = GetConnectionString(tutorialConfiguration, TptConnectionStringName);
        var tpcConnectionString = GetConnectionString(tutorialConfiguration, TpcConnectionStringName);

        TutorialConsole.WriteHeader("07", "DDD-rich inheritance mapping");
        TutorialConsole.WriteContext(
            ("Provider", "SQLite"),
            ("TPH", SqliteConnectionStrings.GetDisplayDataSource(tphConnectionString, AppContext.BaseDirectory)),
            ("TPT", SqliteConnectionStrings.GetDisplayDataSource(tptConnectionString, AppContext.BaseDirectory)),
            ("TPC", SqliteConnectionStrings.GetDisplayDataSource(tpcConnectionString, AppContext.BaseDirectory)),
            ("Modelo", "LearningResource aggregate + value objects"));
        TutorialConsole.WriteQuestion(
            "Como o mesmo modelo DDD muda fisicamente quando EF Core 10 usa TPH, TPT ou TPC?");
        TutorialConsole.WriteHypothesis(
            "O dominio protege as regras de publicacao antes de qualquer banco existir.",
            "TPH guarda toda a hierarquia em uma tabela com discriminador.",
            "TPT divide base e derivados em tabelas ligadas por chave.",
            "TPC cria uma tabela por tipo concreto e repete as colunas herdadas.");

        DemonstrateDomainRules();

        await RunMappingAsync<TphLearningCatalogContext>(
            2,
            "TPH",
            "Table per Hierarchy",
            tphConnectionString,
            static options => new TphLearningCatalogContext(options),
            ValidateTph,
            cancellationToken);

        await RunMappingAsync<TptLearningCatalogContext>(
            3,
            "TPT",
            "Table per Type",
            tptConnectionString,
            static options => new TptLearningCatalogContext(options),
            ValidateTpt,
            cancellationToken);

        await RunMappingAsync<TpcLearningCatalogContext>(
            4,
            "TPC",
            "Table per Concrete Type",
            tpcConnectionString,
            static options => new TpcLearningCatalogContext(options),
            ValidateTpc,
            cancellationToken);

        TutorialConsole.WriteConclusion(
            "O modelo de dominio permaneceu igual; apenas a estrategia de mapeamento mudou o schema e o SQL gerados.",
            TutorialConclusionKind.Success);
    }

    private static string GetConnectionString(TutorialConfigurationResult tutorialConfiguration, string name) =>
        SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            name,
            tutorialConfiguration.DirectoryPath);

    private static void DemonstrateDomainRules()
    {
        TutorialConsole.WriteExperiment(
            1,
            "Regra DDD antes do EF Core",
            "Criar um artigo curto demais e tentar publica-lo pelo aggregate.");
        TutorialConsole.WriteCodeSnippet(
            "A publicacao passa pelo metodo de dominio, nao por um setter.",
            "Models/LearningResources.cs",
            """
            var draft = ArticleResource.Create(
                ResourceTitle.Create("Short EF Core note"),
                InstructorName.Create("Ada Lovelace"),
                LearningLevel.Beginner,
                WordCount.From(120),
                DateTime.UtcNow);

            draft.Publish(DateTime.UtcNow);
            """);

        var draft = ArticleResource.Create(
            ResourceTitle.Create("Short EF Core note"),
            InstructorName.Create("Ada Lovelace"),
            LearningLevel.Beginner,
            WordCount.From(120),
            DateTime.UtcNow);

        try
        {
            draft.Publish(DateTime.UtcNow);
        }
        catch (DomainException exception)
        {
            TutorialConsole.WriteEvidence(
                "Invariante protegida pelo modelo",
                ("Tipo", draft.ResourceKind),
                ("Publicado?", FormatBoolean(draft.IsPublished)),
                ("Erro", exception.Message));
            TutorialConsole.WriteConclusion(
                "O EF Core ainda nao entrou em cena; a regra ja foi bloqueada pelo aggregate.",
                TutorialConclusionKind.Success);
            return;
        }

        throw new InvalidOperationException("The short article should not be publishable.");
    }

    private static async Task RunMappingAsync<TContext>(
        int experimentNumber,
        string abbreviation,
        string mappingName,
        string connectionString,
        Func<DbContextOptions<TContext>, TContext> createContext,
        Func<IReadOnlyCollection<SqliteSchemaObject>, string> validateSchema,
        CancellationToken cancellationToken)
        where TContext : LearningCatalogContext
    {
        TutorialConsole.WriteExperiment(
            experimentNumber,
            $"{abbreviation} - {mappingName}",
            "Recriar o banco, salvar os mesmos recursos publicados e observar schema, SQL e materializacao polimorfica.");

        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlite(connectionString)
            .Options;

        await using var context = createContext(options);
        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        context.AddRange(CreatePublishedResources());
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        var query = context.Resources
            .AsNoTracking()
            .OrderBy(resource => resource.CreatedOnUtc);
        var sql = query.ToQueryString();
        var resources = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        var schema = await ReadSchemaAsync(context, cancellationToken).ConfigureAwait(false);
        var schemaConclusion = validateSchema(schema);

        TutorialConsole.WriteCodeSnippet(
            $"Consulta polimorfica gerada para {abbreviation}.",
            $"{typeof(TContext).Name}.sql",
            sql);
        TutorialConsole.WriteCodeSnippet(
            $"Schema SQLite criado por {abbreviation}.",
            "sqlite_master",
            FormatSchema(schema));
        TutorialConsole.WriteEvidence(
            $"{abbreviation} observado",
            ("Tabelas", string.Join(", ", schema.Select(item => item.Name))),
            ("Total via base", resources.Count.ToString()),
            ("Artigos", resources.OfType<ArticleResource>().Count().ToString()),
            ("Videos", resources.OfType<VideoResource>().Count().ToString()),
            ("Workshops", resources.OfType<LiveWorkshopResource>().Count().ToString()),
            ("Tipos materializados", string.Join(", ", resources.Select(resource => resource.GetType().Name))),
            ("Detalhes", string.Join(" | ", resources.Select(resource => $"{resource.Title}: {resource.FormatSpecifics()}"))),
            ("Validacao do schema", schemaConclusion));
        TutorialConsole.WriteConclusion(
            DescribeMapping(abbreviation),
            TutorialConclusionKind.Success);

        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
    }

    private static LearningResource[] CreatePublishedResources()
    {
        var createdOnUtc = DateTime.SpecifyKind(new DateTime(2026, 6, 22, 12, 0, 0), DateTimeKind.Utc);
        var publishedOnUtc = createdOnUtc.AddHours(2);
        LearningResource[] resources =
        [
            ArticleResource.Create(
                ResourceTitle.Create("EF Core inheritance mapping guide"),
                InstructorName.Create("Ada Lovelace"),
                LearningLevel.Intermediate,
                WordCount.From(1_200),
                createdOnUtc),
            VideoResource.Create(
                ResourceTitle.Create("Mapping strategies in practice"),
                InstructorName.Create("Grace Hopper"),
                LearningLevel.Advanced,
                VideoDuration.FromMinutes(18),
                createdOnUtc.AddMinutes(1)),
            LiveWorkshopResource.Create(
                ResourceTitle.Create("Hands-on EF Core hierarchy lab"),
                InstructorName.Create("Barbara Liskov"),
                LearningLevel.Beginner,
                SeatLimit.From(12),
                createdOnUtc.AddMinutes(2))
        ];

        foreach (var resource in resources)
        {
            resource.Publish(publishedOnUtc);
        }

        return resources;
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
                SELECT name, sql
                FROM sqlite_master
                WHERE type = 'table' AND name NOT LIKE 'sqlite_%'
                ORDER BY name;
                """;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                schema.Add(new SqliteSchemaObject(reader.GetString(0), reader.GetString(1)));
            }

            return schema;
        }
        finally
        {
            await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        }
    }

    private static string ValidateTph(IReadOnlyCollection<SqliteSchemaObject> schema)
    {
        var tables = GetTableNames(schema);
        EnsureTables("TPH", tables, required: ["LearningResources"], forbidden: ["Articles", "Videos", "LiveWorkshops"]);
        var ddl = schema.Single(item => item.Name == "LearningResources").Sql;

        if (!ddl.Contains("ResourceType", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("TPH schema should include the ResourceType discriminator column.");

        return "uma tabela com discriminador ResourceType";
    }

    private static string ValidateTpt(IReadOnlyCollection<SqliteSchemaObject> schema)
    {
        EnsureTables("TPT", GetTableNames(schema), required: ["LearningResources", "Articles", "Videos", "LiveWorkshops"], forbidden: []);
        return "tabela base mais uma tabela por tipo derivado";
    }

    private static string ValidateTpc(IReadOnlyCollection<SqliteSchemaObject> schema)
    {
        EnsureTables("TPC", GetTableNames(schema), required: ["Articles", "Videos", "LiveWorkshops"], forbidden: ["LearningResources"]);
        return "somente tabelas concretas, com colunas herdadas repetidas";
    }

    private static IReadOnlySet<string> GetTableNames(IEnumerable<SqliteSchemaObject> schema) =>
        schema.Select(item => item.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static void EnsureTables(
        string mapping,
        IReadOnlySet<string> tables,
        IReadOnlyCollection<string> required,
        IReadOnlyCollection<string> forbidden)
    {
        var missing = required.Where(table => !tables.Contains(table)).ToArray();
        var unexpected = forbidden.Where(tables.Contains).ToArray();

        if (missing is [] && unexpected is [])
            return;

        throw new InvalidOperationException(
            $"{mapping} schema mismatch. Missing: {FormatList(missing)}. Unexpected: {FormatList(unexpected)}.");
    }

    private static string FormatSchema(IEnumerable<SqliteSchemaObject> schema) =>
        string.Join(
            $"{Environment.NewLine}{Environment.NewLine}",
            schema.Select(item => $"-- {item.Name}{Environment.NewLine}{item.Sql};"));

    private static string DescribeMapping(string abbreviation) => abbreviation switch
    {
        "TPH" => "TPH favorece schema simples e consulta sem joins, ao custo de uma tabela larga com discriminador.",
        "TPT" => "TPT normaliza a hierarquia em varias tabelas, mas a consulta precisa juntar base e derivados.",
        "TPC" => "TPC evita joins para tipos concretos, mas duplica colunas herdadas em cada tabela concreta.",
        _ => throw new ArgumentOutOfRangeException(nameof(abbreviation), abbreviation, "Unknown mapping abbreviation.")
    };

    private static string FormatBoolean(bool value) => value ? "sim" : "nao";

    private static string FormatList(IReadOnlyCollection<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);

    private sealed record SqliteSchemaObject(string Name, string Sql);
}
