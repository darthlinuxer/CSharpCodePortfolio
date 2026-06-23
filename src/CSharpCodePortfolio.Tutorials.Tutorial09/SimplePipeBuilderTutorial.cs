using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial09;

[Tutorial("09", "simple-pipe-builder", "Pipe Builder para decisões com tipos simples")]
public sealed class SimplePipeBuilderTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("09", "Pipe Builder para decisões com tipos simples");
        TutorialConsole.WriteContext(
            ("Tema", "Pipe Builder"),
            ("Conceito", "Uma cadeia de condições substitui decisões repetidas sobre uma entrada simples"),
            ("Runtime", ".NET 10"),
            ("Slug", "simple-pipe-builder"));
        TutorialConsole.WriteQuestion("Como trocar vários `if` sobre uma string por uma cadeia de condições?");
        TutorialConsole.WriteHypothesis(
            "Cada condição sabe testar e tratar uma entrada.",
            "O builder define a ordem das condições disponíveis.",
            "A execução retorna apenas as condições verdadeiras para a entrada recebida.");
        TutorialConsole.WritePreparation(
            "A entrada usada no tutorial é `isStop`.",
            "A cadeia possui três condições: início, teste e parada.",
            "Somente a condição compatível executa sua ação.");

        const string input = "isStop";

        TutorialConsole.WriteExperiment(
            1,
            "Decisão direta",
            "Compara a mesma string em blocos condicionais independentes.");
        TutorialConsole.WriteCodeSnippet(
            "Cada novo valor adiciona mais uma decisão ao mesmo ponto do código.",
            "DirectStringChecks.cs",
            """
            if (input == "isInit") handledMessages.Add("Entrada isInit");
            if (input == "isTest") handledMessages.Add("Entrada isTest");
            if (input == "isStop") handledMessages.Add("Entrada isStop");
            """);

        var directMessages = ExecuteDirectChecks(input);
        TutorialConsole.WriteEvidence(
            "Decisão direta",
            ("Entrada", input),
            ("Ações executadas", string.Join(" | ", directMessages)));

        TutorialConsole.WriteExperiment(
            2,
            "Cadeia com Pipe Builder",
            "Registra tipos de condição, constrói a cadeia e executa apenas os testes aprovados.");
        TutorialConsole.WriteCodeSnippet(
            "O builder concentra a lista de condições disponíveis para a entrada.",
            "IfBuilder.cs",
            """
            var matchedConditions = new IfBuilder(input)
                .AddCheckCondition(typeof(InputIsInitCondition))
                .AddCheckCondition(typeof(InputIsTestCondition))
                .AddCheckCondition(typeof(InputIsStopCondition))
                .Build();
            """);

        var pipeReport = ExecutePipeBuilder(input);
        TutorialConsole.WriteEvidence(
            "Pipe Builder",
            ("Condições registradas", string.Join(" | ", pipeReport.RegisteredConditions)),
            ("Condições aprovadas", string.Join(" | ", pipeReport.MatchedConditions)),
            ("Ações executadas", string.Join(" | ", pipeReport.Messages)));

        TutorialConsole.WriteObservation(
            "A cadeia separa a lista de possibilidades da execução: cada condição continua pequena e o builder controla a composição.");
        TutorialConsole.WriteConclusion(
            "O contrato mínimo é uma condição base, tipos concretos para cada valor e um builder que instancia e filtra as condições.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static IReadOnlyList<string> ExecuteDirectChecks(string input)
    {
        var handledMessages = new List<string>();

        if (input == "isInit")
        {
            handledMessages.Add("Entrada isInit");
        }

        if (input == "isTest")
        {
            handledMessages.Add("Entrada isTest");
        }

        if (input == "isStop")
        {
            handledMessages.Add("Entrada isStop");
        }

        return handledMessages;
    }

    private static PipeBuilderReport ExecutePipeBuilder(string input)
    {
        var builder = new IfBuilder(input)
            .AddCheckCondition(typeof(InputIsInitCondition))
            .AddCheckCondition(typeof(InputIsTestCondition))
            .AddCheckCondition(typeof(InputIsStopCondition));

        var matchedConditions = builder.Build().ToArray();

        return new PipeBuilderReport(
            builder.RegisteredConditionNames,
            matchedConditions.Select(static condition => condition.Name).ToArray(),
            matchedConditions.Select(static condition => condition.Handle()).ToArray());
    }

    private sealed class IfBuilder(string input)
    {
        private readonly List<Type> conditionTypes = [];

        public IReadOnlyList<string> RegisteredConditionNames =>
            conditionTypes.Select(CreateCondition).Select(static condition => condition.Name).ToArray();

        public IfBuilder AddCheckCondition(Type conditionType)
        {
            if (!conditionType.IsSubclassOf(typeof(StringCondition)))
            {
                throw new ArgumentException("O tipo precisa derivar de StringCondition.", nameof(conditionType));
            }

            conditionTypes.Add(conditionType);
            return this;
        }

        public IEnumerable<StringCondition> Build()
        {
            foreach (var conditionType in conditionTypes)
            {
                var condition = CreateCondition(conditionType);
                if (condition.IsMatch)
                {
                    yield return condition;
                }
            }
        }

        private StringCondition CreateCondition(Type conditionType) =>
            (StringCondition)Activator.CreateInstance(conditionType, input)!;
    }

    private abstract class StringCondition(string input)
    {
        protected string Input { get; } = input;

        public abstract string Name { get; }

        public abstract bool IsMatch { get; }

        public abstract string Handle();
    }

    private sealed class InputIsInitCondition(string input) : StringCondition(input)
    {
        public override string Name => "Entrada é isInit";

        public override bool IsMatch => Input == "isInit";

        public override string Handle() => "Entrada isInit";
    }

    private sealed class InputIsTestCondition(string input) : StringCondition(input)
    {
        public override string Name => "Entrada é isTest";

        public override bool IsMatch => Input == "isTest";

        public override string Handle() => "Entrada isTest";
    }

    private sealed class InputIsStopCondition(string input) : StringCondition(input)
    {
        public override string Name => "Entrada é isStop";

        public override bool IsMatch => Input == "isStop";

        public override string Handle() => "Entrada isStop";
    }

    private sealed record PipeBuilderReport(
        IReadOnlyList<string> RegisteredConditions,
        IReadOnlyList<string> MatchedConditions,
        IReadOnlyList<string> Messages);
}
