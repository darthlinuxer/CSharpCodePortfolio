using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.TutorialTemplate;

[Tutorial("TutorialId", "TutorialSlug", "TutorialTitle")]
public sealed class TutorialClass : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("TutorialId", "TutorialTitle");
        TutorialConsole.WriteContext(
            ("Status", "Template inicial"),
            ("Slug", "TutorialSlug"));
        TutorialConsole.WriteQuestion("Qual conceito este tutorial deve ensinar?");
        TutorialConsole.WriteHypothesis("O tutorial deve executar exemplos pequenos e mostrar evidencia no console.");
        TutorialConsole.WritePreparation("Substitua este corpo pelo menor exemplo que prove o conceito.");
        TutorialConsole.WriteConclusion("Template criado; a migracao real deve preservar o conceito da pasta antiga.");

        return Task.CompletedTask;
    }
}
