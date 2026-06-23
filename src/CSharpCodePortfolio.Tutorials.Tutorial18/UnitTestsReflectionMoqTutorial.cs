using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial18;

[Tutorial("18", "unit-tests-reflection-moq", "Testes por Reflexão com Moq")]
public sealed class UnitTestsReflectionMoqTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("18", "Testes por Reflexão com Moq");
        TutorialConsole.WriteContext(
            ("Tema", "Testes automatizados"),
            ("Conceito", "Descobrir métodos por reflexão e isolar dependências com Moq"),
            ("Runtime", ".NET 10"),
            ("Slug", "unit-tests-reflection-moq"));
        TutorialConsole.WriteQuestion("Como executar testes descobertos por reflexão e validar uma dependência mockada?");
        TutorialConsole.WriteHypothesis(
            "Um atributo marca quais métodos representam testes executáveis.",
            "A reflexão encontra esses métodos e injeta os argumentos declarados no atributo.",
            "Moq substitui a dependência e verifica se o fluxo chamou o contrato esperado.");
        TutorialConsole.WritePreparation(
            "A máquina de cálculo depende de `ICalculator`.",
            "`CalculatorMachineTests` contém métodos anotados com `PortfolioTestAttribute`.",
            "Cada método configura um `Mock<ICalculator>`, executa o fluxo e valida a interação.",
            "`InternalsVisibleTo` libera os tipos internos para o proxy dinâmico do Moq.");

        TutorialConsole.WriteExperiment(
            1,
            "Descoberta por reflexão",
            "Localiza métodos anotados e executa cada um com os argumentos do atributo.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a descoberta fica explícita em `GetMethods` e `GetCustomAttribute`.",
            "ReflectionTestRunner.cs",
            """
            var methods = typeof(CalculatorMachineTests)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(method => method.IsDefined(typeof(PortfolioTestAttribute), inherit: false));

            foreach (var method in methods)
            {
                var test = method.GetCustomAttribute<PortfolioTestAttribute>()!;
                method.Invoke(null, [test.First, test.Second]);
            }
            """);

        TutorialConsole.WriteExperiment(
            2,
            "Dependência mockada",
            "Configura o retorno da calculadora e verifica a chamada esperada.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: `Setup` define o comportamento e `Verify` valida a interação.",
            "CalculatorMachineTests.cs",
            """
            var calculator = new Mock<ICalculator>(MockBehavior.Strict);
            calculator
                .Setup(calc => calc.Calculate(It.IsAny<Operation>(), first, second))
                .Returns((Operation operation, double a, double b) => operation(a, b));

            var result = action(new CalculatorMachine(calculator.Object));

            calculator.Verify(
                calc => calc.Calculate(It.IsAny<Operation>(), first, second),
                Times.Once);
            """);

        var report = ReflectionTestRunner.Run();
        VerifyReport(report);

        TutorialConsole.WriteEvidence(
            "Execução dos testes",
            ("Classe", report.TestClassName),
            ("Testes descobertos", report.DiscoveredTests.ToString()),
            ("Testes aprovados", report.PassedTests.ToString()));

        foreach (var testCase in report.Cases)
        {
            TutorialConsole.WriteEvidence(
                testCase.TestName,
                ("Cenário", testCase.Scenario),
                ("Resultado", testCase.Result.ToString("0.##")),
                ("Interação verificada", testCase.VerifiedInteraction));
        }

        TutorialConsole.WriteObservation(
            "A reflexão descobre o que deve ser executado; o mock prova como o objeto conversa com a dependência.");
        TutorialConsole.WriteConclusion(
            "O teste fica focado no comportamento: dados de entrada vêm do atributo, a dependência é controlada por Moq e a execução valida resultado e interação.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static void VerifyReport(TestRunReport report)
    {
        if (report.DiscoveredTests != 2 || report.PassedTests != 2)
        {
            throw new InvalidOperationException("A execução deve descobrir e aprovar dois testes.");
        }

        if (report.Cases.Any(static testCase => testCase.Result <= 0))
        {
            throw new InvalidOperationException("Os cenários devem produzir resultados positivos.");
        }
    }
}
