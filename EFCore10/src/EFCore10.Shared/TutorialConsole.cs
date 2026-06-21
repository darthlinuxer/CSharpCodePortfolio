using Spectre.Console;

namespace EFCore10.Shared;

/// <summary>
/// Writes tutorial narratives with a consistent console layout.
/// </summary>
public static class TutorialConsole
{
    /// <summary>
    /// Writes the tutorial title.
    /// </summary>
    public static void WriteHeader(string tutorialId, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tutorialId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new Rule($"[bold blue]Tutorial {Escape(tutorialId)}[/] [grey]|[/] [bold]{Escape(title)}[/]")
                .RuleStyle("grey")
                .LeftJustified());
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Writes contextual information that applies to the whole tutorial run.
    /// </summary>
    public static void WriteContext(params (string Label, string Value)[] items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var table = CreateKeyValueTable(items, "grey");

        AnsiConsole.Write(
            new Panel(table)
                .Header("[bold]Contexto[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Grey)
                .Expand());
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Writes measured evidence collected by the current experiment.
    /// </summary>
    public static void WriteEvidence(string title, params (string Label, string Value)[] items)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(items);

        var table = CreateKeyValueTable(items, "aqua");

        AnsiConsole.Write(
            new Panel(table)
                .Header($"[bold]Evidências[/] [grey]|[/] {Escape(title)}")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Aqua)
                .Expand());
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Writes the question that drives the tutorial.
    /// </summary>
    public static void WriteQuestion(string question)
    {
        WritePanel("Pergunta central", question, Color.Blue);
    }

    /// <summary>
    /// Writes the hypothesis that will be tested by the experiments.
    /// </summary>
    public static void WriteHypothesis(params string[] lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        WritePanel("Hipótese", FormatLines(lines, includeBullets: true), Color.Yellow);
    }

    /// <summary>
    /// Writes setup observations that are necessary before the experiments.
    /// </summary>
    public static void WritePreparation(params string[] observations)
    {
        WriteSection("Preparação");
        WriteBullets(observations);
    }

    /// <summary>
    /// Writes an experiment heading and its action.
    /// </summary>
    public static void WriteExperiment(int number, string title, string action)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number), number, "The experiment number must be greater than zero.");

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new Rule($"[bold yellow]Experimento {number}[/] [grey]|[/] {Escape(title)}")
                .RuleStyle("grey")
                .LeftJustified());
        WriteLabeledText("Ação", action, "blue");
    }

    /// <summary>
    /// Writes a curated code snippet related to the current experiment.
    /// </summary>
    public static void WriteCodeSnippet(string title, string fileName, string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        WriteLabeledText("Código", title, "purple");

        var panel = new Panel(new Markup($"[grey]{Escape(NormalizeCode(code))}[/]"))
            .Header($"[bold]Código observado[/] [grey]|[/] {Escape(fileName)}")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Purple)
            .Expand();

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Writes a runtime observation from the current experiment.
    /// </summary>
    public static void WriteObservation(string observation)
    {
        WriteLabeledText("Observação", observation, "grey");
    }

    /// <summary>
    /// Writes the conclusion for the current experiment.
    /// </summary>
    public static void WriteConclusion(
        string conclusion,
        TutorialConclusionKind kind = TutorialConclusionKind.Neutral)
    {
        var color = kind switch
        {
            TutorialConclusionKind.Success => "green",
            TutorialConclusionKind.Warning => "yellow",
            TutorialConclusionKind.Failure => "red",
            _ => "white"
        };

        WriteLabeledText("Conclusão", conclusion, color);
    }

    /// <summary>
    /// Writes cleanup observations after the experiments.
    /// </summary>
    public static void WriteCleanup(params string[] observations)
    {
        WriteSection("Limpeza");
        WriteBullets(observations);
    }

    private static void WritePanel(string header, string text, Color borderColor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        AnsiConsole.Write(
            new Panel(new Markup(Escape(text)))
                .Header($"[bold]{Escape(header)}[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(borderColor)
                .Expand());
        AnsiConsole.WriteLine();
    }

    private static void WriteSection(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new Rule($"[bold]{Escape(title)}[/]")
                .RuleStyle("grey")
                .LeftJustified());
    }

    private static void WriteBullets(IReadOnlyCollection<string> observations)
    {
        ArgumentNullException.ThrowIfNull(observations);

        var table = new Table()
            .NoBorder()
            .HideHeaders()
            .AddColumn(string.Empty)
            .AddColumn(string.Empty);

        foreach (var observation in observations.Where(static observation => !string.IsNullOrWhiteSpace(observation)))
        {
            table.AddRow(
                new Markup("[grey]-[/]"),
                new Markup(Escape(observation)));
        }

        AnsiConsole.Write(table);
    }

    private static Table CreateKeyValueTable(
        IReadOnlyCollection<(string Label, string Value)> items,
        string labelColor)
    {
        var table = new Table()
            .NoBorder()
            .HideHeaders()
            .AddColumn("Campo")
            .AddColumn("Valor");

        foreach (var (label, value) in items)
        {
            if (string.IsNullOrWhiteSpace(label))
                continue;

            table.AddRow(
                new Markup($"[bold {Escape(labelColor)}]{Escape(label)}[/]"),
                new Markup(Escape(value)));
        }

        return table;
    }

    private static void WriteLabeledText(string label, string text, string labelColor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(labelColor);

        var table = new Table()
            .NoBorder()
            .HideHeaders()
            .AddColumn("Rótulo")
            .AddColumn("Texto");

        table.AddRow(
            new Markup($"[bold {Escape(labelColor)}]{Escape(label)}[/]"),
            new Markup(Escape(text)));

        AnsiConsole.Write(table);
    }

    private static string FormatLines(IReadOnlyCollection<string> lines, bool includeBullets)
    {
        return string.Join(
            Environment.NewLine,
            lines
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => includeBullets ? $"- {line}" : line));
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim('\r', '\n');
    }

    private static string Escape(string value)
    {
        return Markup.Escape(value);
    }
}
