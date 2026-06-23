using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial10;

[Tutorial("10", "complex-pipe-builder", "Pipe Builder para decisões com objetos complexos")]
public sealed class ComplexPipeBuilderTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("10", "Pipe Builder para decisões com objetos complexos");
        TutorialConsole.WriteContext(
            ("Tema", "Pipe Builder"),
            ("Conceito", "Uma cadeia de condições substitui decisões repetidas sobre um objeto de domínio"),
            ("Runtime", ".NET 10"),
            ("Slug", "complex-pipe-builder"));
        TutorialConsole.WriteQuestion("Como trocar vários `if` sobre propriedades de um objeto por uma cadeia de condições?");
        TutorialConsole.WriteHypothesis(
            "Cada condição recebe o objeto inteiro e decide se deve agir.",
            "O builder mantém a ordem das condições disponíveis.",
            "A execução retorna somente as condições aprovadas para o estado atual do objeto.");
        TutorialConsole.WritePreparation(
            "A pessoa usada no tutorial possui nome e idade.",
            "A cadeia possui condições para Camilo e Aline.",
            "Somente a condição compatível com o objeto executa sua ação.");

        var person = new Person("Camilo", 37);

        TutorialConsole.WriteExperiment(
            1,
            "Decisão direta",
            "Consulta propriedades do mesmo objeto em blocos condicionais independentes.");
        TutorialConsole.WriteCodeSnippet(
            "Cada nova regra adiciona mais uma decisão ao mesmo ponto do código.",
            "DirectPersonChecks.cs",
            """
            if (person.Name == "Camilo") messages.Add("Pessoa identificada como Camilo");
            if (person.Name == "Aline") messages.Add("Pessoa identificada como Aline");
            """);

        var directMessages = ExecuteDirectChecks(person);
        TutorialConsole.WriteEvidence(
            "Decisão direta",
            ("Pessoa", Describe(person)),
            ("Ações executadas", string.Join(" | ", directMessages)));

        TutorialConsole.WriteExperiment(
            2,
            "Cadeia com Pipe Builder",
            "Registra tipos de condição, constrói a cadeia e executa apenas os testes aprovados.");
        TutorialConsole.WriteCodeSnippet(
            "O builder concentra a composição das regras sem misturar as ações em um bloco condicional único.",
            "IfBuilder.cs",
            """
            var matchedConditions = new IfBuilder(person)
                .AddCheckCondition(typeof(PersonNameIsCamiloCondition))
                .AddCheckCondition(typeof(PersonNameIsAlineCondition))
                .Build();
            """);

        var pipeReport = ExecutePipeBuilder(person);
        TutorialConsole.WriteEvidence(
            "Pipe Builder",
            ("Condições registradas", string.Join(" | ", pipeReport.RegisteredConditions)),
            ("Condições aprovadas", string.Join(" | ", pipeReport.MatchedConditions)),
            ("Ações executadas", string.Join(" | ", pipeReport.Messages)));

        TutorialConsole.WriteObservation(
            "A cadeia deixa cada regra perto da ação correspondente e mantém a composição em um único ponto.");
        TutorialConsole.WriteConclusion(
            "Quando a decisão depende de um objeto completo, uma condição base e tipos concretos mantêm as regras pequenas e testáveis.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static IReadOnlyList<string> ExecuteDirectChecks(Person person)
    {
        var messages = new List<string>();

        if (person.Name == "Camilo")
        {
            messages.Add("Pessoa identificada como Camilo");
        }

        if (person.Name == "Aline")
        {
            messages.Add("Pessoa identificada como Aline");
        }

        return messages;
    }

    private static PipeBuilderReport ExecutePipeBuilder(Person person)
    {
        var builder = new IfBuilder(person)
            .AddCheckCondition(typeof(PersonNameIsCamiloCondition))
            .AddCheckCondition(typeof(PersonNameIsAlineCondition));

        var matchedConditions = builder.Build().ToArray();

        return new PipeBuilderReport(
            builder.RegisteredConditionNames,
            matchedConditions.Select(static condition => condition.Name).ToArray(),
            matchedConditions.Select(static condition => condition.Handle()).ToArray());
    }

    private static string Describe(Person person) =>
        $"Nome: {person.Name}, idade: {person.Age}";

    private sealed class IfBuilder(Person person)
    {
        private readonly List<Type> conditionTypes = [];

        public IReadOnlyList<string> RegisteredConditionNames =>
            conditionTypes.Select(CreateCondition).Select(static condition => condition.Name).ToArray();

        public IfBuilder AddCheckCondition(Type conditionType)
        {
            if (!conditionType.IsSubclassOf(typeof(PersonCondition)))
            {
                throw new ArgumentException("O tipo precisa derivar de PersonCondition.", nameof(conditionType));
            }

            conditionTypes.Add(conditionType);
            return this;
        }

        public IEnumerable<PersonCondition> Build()
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

        private PersonCondition CreateCondition(Type conditionType) =>
            (PersonCondition)Activator.CreateInstance(conditionType, person)!;
    }

    private abstract class PersonCondition(Person person)
    {
        protected Person Person { get; } = person;

        public abstract string Name { get; }

        public abstract bool IsMatch { get; }

        public abstract string Handle();
    }

    private sealed class PersonNameIsCamiloCondition(Person person) : PersonCondition(person)
    {
        public override string Name => "Pessoa chamada Camilo";

        public override bool IsMatch => Person.Name == "Camilo";

        public override string Handle() => "Pessoa identificada como Camilo";
    }

    private sealed class PersonNameIsAlineCondition(Person person) : PersonCondition(person)
    {
        public override string Name => "Pessoa chamada Aline";

        public override bool IsMatch => Person.Name == "Aline";

        public override string Handle() => "Pessoa identificada como Aline";
    }

    private sealed record Person(string Name, int Age);

    private sealed record PipeBuilderReport(
        IReadOnlyList<string> RegisteredConditions,
        IReadOnlyList<string> MatchedConditions,
        IReadOnlyList<string> Messages);
}
