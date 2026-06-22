using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using System.Text.RegularExpressions;

namespace CSharpCodePortfolio.Tutorials.Tutorial06;

[Tutorial("06", "lambda-validators", "Validadores com expressões lambda")]
public sealed class LambdaValidatorsTutorial : ITutorial
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly (string Name, Predicate<Person> Rule)[] Rules =
    [
        ("Nome informado", person => !string.IsNullOrWhiteSpace(person.Name)),
        ("Idade positiva", person => person.Age > 0),
        ("E-mail em formato básico", person => EmailRegex.IsMatch(person.Email))
    ];

    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("06", "Validadores com expressões lambda");
        TutorialConsole.WriteContext(
            ("Tema", "Validação funcional"),
            ("Conceito", "Cada regra é uma função que decide se o objeto é válido"),
            ("Runtime", ".NET 10"),
            ("Slug", "lambda-validators"));
        TutorialConsole.WriteQuestion("Como compor validações pequenas usando expressões lambda?");
        TutorialConsole.WriteHypothesis(
            "Uma regra pode ser representada por `Predicate<T>`.",
            "A validação completa é verdadeira apenas quando todas as regras retornam `true`.",
            "Nomear cada regra torna o resultado mais útil do que um booleano isolado.");
        TutorialConsole.WritePreparation(
            "O exemplo valida uma pessoa com nome, idade e e-mail.",
            "As regras são mantidas em uma lista e executadas em sequência.",
            "O resultado mostra quais regras passaram e quais falharam.");

        TutorialConsole.WriteExperiment(
            1,
            "Regras como lambdas",
            "Define predicados pequenos que podem ser combinados para validar o mesmo objeto.");
        TutorialConsole.WriteCodeSnippet(
            "Cada lambda recebe uma pessoa e retorna `true` ou `false`.",
            "PersonRules.cs",
            """
            private static readonly (string Name, Predicate<Person> Rule)[] Rules =
            [
                ("Nome informado", person => !string.IsNullOrWhiteSpace(person.Name)),
                ("Idade positiva", person => person.Age > 0),
                ("E-mail em formato básico", person => EmailRegex.IsMatch(person.Email))
            ];
            """);

        var validReport = Validate(new Person("Camila", 32, "camila@example.com"));
        var invalidReport = Validate(new Person("Camilo", -1, "camilo@example.com"));

        TutorialConsole.WriteEvidence(
            "Pessoa aprovada",
            ("Entrada", Describe(validReport.Person)),
            ("Resultado", validReport.IsValid ? "válida" : "inválida"),
            ("Regras", DescribeRules(validReport)));

        TutorialConsole.WriteExperiment(
            2,
            "Composição das regras",
            "Executa todas as regras e calcula o resultado final com `All`.");
        TutorialConsole.WriteCodeSnippet(
            "O resultado final depende de todas as regras.",
            "Validator.cs",
            """
            var outcomes = Rules
                .Select(rule => new RuleOutcome(rule.Name, rule.Rule(person)))
                .ToArray();

            var isValid = outcomes.All(static outcome => outcome.Passed);
            """);
        TutorialConsole.WriteEvidence(
            "Pessoa reprovada",
            ("Entrada", Describe(invalidReport.Person)),
            ("Resultado", invalidReport.IsValid ? "válida" : "inválida"),
            ("Regras", DescribeRules(invalidReport)));

        TutorialConsole.WriteObservation(
            "Predicados deixam a regra perto do dado validado e permitem reaproveitar a composição para outros objetos do mesmo tipo.");
        TutorialConsole.WriteConclusion(
            "O contrato mínimo é uma coleção de predicados e uma política clara para combinar os resultados.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static ValidationReport Validate(Person person)
    {
        var outcomes = Rules
            .Select(rule => new RuleOutcome(rule.Name, rule.Rule(person)))
            .ToArray();

        return new ValidationReport(person, outcomes);
    }

    private static string Describe(Person person) =>
        $"Nome: {person.Name}, idade: {person.Age}, e-mail: {person.Email}";

    private static string DescribeRules(ValidationReport report) =>
        string.Join(" | ", report.Outcomes.Select(static outcome =>
            $"{outcome.Name}: {(outcome.Passed ? "ok" : "falhou")}"));

    private sealed record Person(string Name, int Age, string Email);

    private sealed record RuleOutcome(string Name, bool Passed);

    private sealed record ValidationReport(Person Person, IReadOnlyList<RuleOutcome> Outcomes)
    {
        public bool IsValid => Outcomes.All(static outcome => outcome.Passed);
    }
}
