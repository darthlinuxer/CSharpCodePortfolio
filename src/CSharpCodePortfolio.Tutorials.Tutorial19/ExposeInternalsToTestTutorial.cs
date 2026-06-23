using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial19;

[Tutorial("19", "expose-internals-to-test", "InternalsVisibleTo para Testes")]
public sealed class ExposeInternalsToTestTutorial : ITutorial
{
    private const string FriendAssemblyName = "CSharpCodePortfolio.Tutorials.Tutorial19.Tests";
    private static readonly CultureInfo BrazilianPortuguese = CultureInfo.GetCultureInfo("pt-BR");

    public Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("19", "InternalsVisibleTo para Testes");
        TutorialConsole.WriteContext(
            ("Tema", "Testabilidade de membros internos"),
            ("Conceito", "Expor tipos `internal` para um assembly de teste controlado"),
            ("Runtime", ".NET 10"),
            ("Slug", "expose-internals-to-test"));
        TutorialConsole.WriteQuestion("Como testar uma regra interna sem transformar a API pública em superfície de teste?");
        TutorialConsole.WriteHypothesis(
            "`internal` mantém a regra fora da API pública do assembly.",
            "`InternalsVisibleTo` declara exatamente qual assembly pode acessar esses membros.",
            "O projeto de teste compila contra a regra interna e valida o comportamento diretamente.");
        TutorialConsole.WritePreparation(
            "`CustomerDiscountPolicy` representa uma regra interna do domínio.",
            "`AssemblyInfo.cs` declara o assembly friend autorizado.",
            "`CSharpCodePortfolio.Tutorials.Tutorial19.Tests` acessa os tipos internos por referência de projeto.");

        TutorialConsole.WriteExperiment(
            1,
            "Abertura controlada do assembly",
            "Aplica o atributo no nível do assembly e informa o nome exato do assembly de teste.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o atributo fica no assembly que contém os tipos internos.",
            "src/CSharpCodePortfolio.Tutorials.Tutorial19/AssemblyInfo.cs");

        var friendAssembly = FindFriendAssembly();

        TutorialConsole.WriteEvidence(
            "Atributo aplicado",
            ("Assembly de origem", typeof(CustomerDiscountPolicy).Assembly.GetName().Name ?? string.Empty),
            ("Assembly autorizado", friendAssembly.AssemblyName),
            ("Tipo protegido", typeof(CustomerDiscountPolicy).Name));

        TutorialConsole.WriteExperiment(
            2,
            "Regra interna testável",
            "Executa a política interna e mostra a mesma regra que o projeto de teste acessa.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a política continua `internal` e concentra a regra de desconto.",
            typeof(CustomerDiscountPolicy));

        var request = new CustomerDiscountRequest(200m, IsLoyalCustomer: true);
        var decision = new CustomerDiscountPolicy().Evaluate(request);

        VerifyDecision(decision);

        TutorialConsole.WriteEvidence(
            "Execução da regra",
            ("Total do pedido", FormatCurrency(request.OrderTotal)),
            ("Percentual aplicado", decision.DiscountRate.ToString("P0", BrazilianPortuguese)),
            ("Desconto", FormatCurrency(decision.DiscountAmount)),
            ("Valor final", FormatCurrency(decision.PayableAmount)),
            ("Motivo", decision.Reason));

        TutorialConsole.WriteExperiment(
            3,
            "Teste em assembly separado",
            "O teste acessa `CustomerDiscountPolicy` diretamente porque o assembly friend foi declarado.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o teste usa tipos internos sem reflexão.",
            "tests/CSharpCodePortfolio.Tutorials.Tutorial19.Tests/CustomerDiscountPolicyTests.cs");

        TutorialConsole.WriteObservation(
            "A API pública não cresce para atender o teste; a permissão fica explícita no metadado do assembly.");
        TutorialConsole.WriteConclusion(
            "`InternalsVisibleTo` é útil quando uma regra deve permanecer interna, mas precisa de testes diretos e legíveis em outro assembly.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static InternalsVisibleToAttribute FindFriendAssembly()
    {
        return typeof(CustomerDiscountPolicy)
            .Assembly
            .GetCustomAttributes<InternalsVisibleToAttribute>()
            .SingleOrDefault(static attribute => string.Equals(
                attribute.AssemblyName,
                FriendAssemblyName,
                StringComparison.Ordinal))
            ?? throw new InvalidOperationException("O assembly de teste autorizado não foi encontrado.");
    }

    private static void VerifyDecision(DiscountDecision decision)
    {
        if (decision.DiscountRate != 0.15m ||
            decision.DiscountAmount != 30m ||
            decision.PayableAmount != 170m)
        {
            throw new InvalidOperationException("A política interna calculou um desconto inesperado.");
        }
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C", BrazilianPortuguese);
    }
}
