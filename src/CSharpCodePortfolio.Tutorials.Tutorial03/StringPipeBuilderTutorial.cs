using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial03;

[Tutorial("03", "string-pipe-builder", "Pipeline de strings com recursão reversa")]
public sealed class StringPipeBuilderTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("03", "Pipeline de strings com recursão reversa");
        TutorialConsole.WriteContext(
            ("Tema", "Pipe builder funcional"),
            ("Conceito", "Compor transformações de string em uma cadeia executável"),
            ("Runtime", ".NET 10"),
            ("Slug", "string-pipe-builder"));
        TutorialConsole.WriteQuestion("Como montar um pipeline em que cada etapa transforma a entrada e chama a próxima etapa?");
        TutorialConsole.WriteHypothesis(
            "Cada pipe pode ser uma função pequena com uma responsabilidade única.",
            "A recursão começa na primeira etapa, mas constrói chamadas aninhadas até chegar à ação final.",
            "A ordem visível de execução deve ser a mesma ordem em que os pipes foram registrados.");
        TutorialConsole.WritePreparation(
            "O pipeline terá duas etapas: remover espaços externos e converter para maiúsculas.",
            "A ação final apenas coleta o resultado para que o tutorial consiga mostrar evidências.",
            "Não há reflexão aqui: delegates são suficientes para ensinar composição de pipes.");

        var builder = new StringPipeBuilder()
            .Add("Trim", static value => value.Trim())
            .Add("Uppercase", static value => value.ToUpperInvariant());

        TutorialConsole.WriteExperiment(
            1,
            "Construção recursiva",
            "O builder cria a ação terminal e volta empilhando cada pipe por cima da próxima ação.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a recursão para no fim da lista e volta compondo as etapas.",
            typeof(StringPipeBuilder),
            "BuildAt",
            new CodeExcerpt(3, 15, "Caso base, próxima etapa e delegate composto"));

        var execution = builder.Run("   Olá, pipes!   ");
        EnsureExpectedResult(execution);

        TutorialConsole.WriteEvidence(
            "Execução",
            ("Entrada", ShowWhitespace(execution.Input)),
            ("Etapas", string.Join(" -> ", execution.Trace)),
            ("Saída", execution.Output));
        TutorialConsole.WriteObservation(
            "A recursão monta a cadeia sem uma classe por pipe; uma função com nome já basta quando a transformação é simples.");
        TutorialConsole.WriteConclusion(
            "Pipeline é composição controlada: cada etapa muda uma coisa, repassa o valor, e a ordem de registro continua fácil de ler.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static void EnsureExpectedResult(PipeExecution execution)
    {
        if (execution.Output != "OLÁ, PIPES!")
        {
            throw new InvalidOperationException($"Pipeline result mismatch: '{execution.Output}'.");
        }
    }

    private static string ShowWhitespace(string value)
    {
        return value.Replace(" ", "·", StringComparison.Ordinal);
    }

    private sealed class StringPipeBuilder
    {
        private readonly List<PipeStep> _steps = [];

        public StringPipeBuilder Add(string name, Func<string, string> transform)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(transform);

            _steps.Add(new PipeStep(name, transform));
            return this;
        }

        public PipeExecution Run(string input)
        {
            ArgumentNullException.ThrowIfNull(input);

            var trace = new List<string>();
            var output = string.Empty;
            var terminal = BuildAt(0, value => output = value, trace);
            terminal(input);

            return new PipeExecution(input, output, trace);
        }

        private Action<string> BuildAt(int index, Action<string> terminal, List<string> trace)
        {
            if (index == _steps.Count)
            {
                return terminal;
            }

            var step = _steps[index];
            var next = BuildAt(index + 1, terminal, trace);

            return value =>
            {
                trace.Add(step.Name);
                next(step.Transform(value));
            };
        }
    }

    private sealed record PipeStep(string Name, Func<string, string> Transform);

    private sealed record PipeExecution(string Input, string Output, IReadOnlyList<string> Trace);
}
