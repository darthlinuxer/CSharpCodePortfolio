using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using System.Reflection;

namespace CSharpCodePortfolio.Tutorials.Tutorial08;

[Tutorial("08", "complex-object-reflection", "Reflexão em condições para objetos complexos")]
public sealed class ComplexObjectReflectionTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("08", "Reflexão em condições para objetos complexos");
        TutorialConsole.WriteContext(
            ("Tema", "Reflexão com objeto de entrada"),
            ("Conceito", "Classes de condição recebem um objeto e decidem se devem executar"),
            ("Runtime", ".NET 10"),
            ("Slug", "complex-object-reflection"));
        TutorialConsole.WriteQuestion("Como trocar vários `if` sobre um objeto por condições descobertas via reflexão?");
        TutorialConsole.WriteHypothesis(
            "Cada condição encapsula uma regra sobre o objeto de entrada.",
            "A reflexão encontra as condições disponíveis sem manter uma lista manual.",
            "O executor chama apenas as condições que retornam verdadeiro para o objeto recebido.");
        TutorialConsole.WritePreparation(
            "O objeto usado no tutorial é uma pessoa com nome e idade.",
            "Duas condições verificam nomes diferentes.",
            "Somente a condição compatível com a pessoa executa a ação.");

        var person = new Person("Camilo", 37);

        TutorialConsole.WriteExperiment(
            1,
            "Decisão direta",
            "Compara propriedades do objeto em blocos condicionais separados.");
        TutorialConsole.WriteCodeSnippet(
            "Cada novo caso aumenta a lista de decisões no mesmo ponto do código.",
            "DirectPersonChecks.cs",
            """
            if (person.Name == "Camilo")
            {
                handledMessages.Add("Pessoa chamada Camilo");
            }

            if (person.Name == "Aline")
            {
                handledMessages.Add("Pessoa chamada Aline");
            }
            """);

        var directMessages = ExecuteDirectChecks(person);
        TutorialConsole.WriteEvidence(
            "Decisão direta",
            ("Entrada", Describe(person)),
            ("Ações executadas", string.Join(" | ", directMessages)));

        TutorialConsole.WriteExperiment(
            2,
            "Condições refletidas",
            "Descobre classes de condição, instancia cada uma com o objeto e executa apenas as verdadeiras.");
        TutorialConsole.WriteCodeSnippet(
            "A busca usa as classes aninhadas que derivam de `PersonCondition`.",
            "ReflectedPersonChecks.cs",
            """
            var conditions = typeof(ComplexObjectReflectionTutorial)
                .GetNestedTypes(BindingFlags.NonPublic)
                .Where(IsConcretePersonCondition)
                .Select(type => (PersonCondition)Activator.CreateInstance(type, person)!);

            var passedConditions = conditions.Where(static condition => condition.IsMatch);
            """);

        var reflectedReport = ExecuteReflectedChecks(person);
        TutorialConsole.WriteEvidence(
            "Reflexão",
            ("Condições descobertas", string.Join(" | ", reflectedReport.DiscoveredConditions)),
            ("Condições aprovadas", string.Join(" | ", reflectedReport.PassedConditions)),
            ("Ações executadas", string.Join(" | ", reflectedReport.Messages)));

        TutorialConsole.WriteObservation(
            "A condição passa a ser uma unidade nomeada: ela conhece o objeto recebido, a regra e a ação correspondente.");
        TutorialConsole.WriteConclusion(
            "O contrato mínimo é uma classe base de condição, um construtor com o objeto de entrada e uma busca refletida por condições concretas.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static IReadOnlyList<string> ExecuteDirectChecks(Person person)
    {
        var handledMessages = new List<string>();

        if (person.Name == "Camilo")
        {
            handledMessages.Add("Pessoa chamada Camilo");
        }

        if (person.Name == "Aline")
        {
            handledMessages.Add("Pessoa chamada Aline");
        }

        return handledMessages;
    }

    private static ReflectedConditionReport ExecuteReflectedChecks(Person person)
    {
        var conditions = typeof(ComplexObjectReflectionTutorial)
            .GetNestedTypes(BindingFlags.NonPublic)
            .Where(IsConcretePersonCondition)
            .Select(type => (PersonCondition)Activator.CreateInstance(type, person)!)
            .ToArray();

        var passedConditions = conditions
            .Where(static condition => condition.IsMatch)
            .ToArray();

        return new ReflectedConditionReport(
            conditions.Select(static condition => condition.Name).ToArray(),
            passedConditions.Select(static condition => condition.Name).ToArray(),
            passedConditions.Select(static condition => condition.Handle()).ToArray());
    }

    private static bool IsConcretePersonCondition(Type type) =>
        type.IsSubclassOf(typeof(PersonCondition)) && !type.IsAbstract;

    private static string Describe(Person person) =>
        $"Nome: {person.Name}, idade: {person.Age}";

    private sealed record Person(string Name, int Age);

    private sealed record ReflectedConditionReport(
        IReadOnlyList<string> DiscoveredConditions,
        IReadOnlyList<string> PassedConditions,
        IReadOnlyList<string> Messages);

    private abstract class PersonCondition(Person person)
    {
        protected Person Person { get; } = person;

        public abstract string Name { get; }

        public abstract bool IsMatch { get; }

        public abstract string Handle();
    }

    private sealed class NameIsCamiloCondition(Person person) : PersonCondition(person)
    {
        public override string Name => "Nome é Camilo";

        public override bool IsMatch => Person.Name == "Camilo";

        public override string Handle() => "Pessoa chamada Camilo";
    }

    private sealed class NameIsAlineCondition(Person person) : PersonCondition(person)
    {
        public override string Name => "Nome é Aline";

        public override bool IsMatch => Person.Name == "Aline";

        public override string Handle() => "Pessoa chamada Aline";
    }
}
