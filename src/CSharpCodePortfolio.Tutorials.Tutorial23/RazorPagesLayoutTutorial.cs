using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial23;

[Tutorial("23", "razor-pages-layout", "Razor Pages com PageModel e Layout")]
public sealed class RazorPagesLayoutTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("23", "Razor Pages com PageModel e Layout");
        TutorialConsole.WriteContext(
            ("Tema", "ASP.NET Core Razor Pages"),
            ("Conceito", "Rota `@page`, `PageModel`, `_ViewStart`, `_ViewImports` e layout compartilhado"),
            ("Runtime", ".NET 10"),
            ("Slug", "razor-pages-layout"));
        TutorialConsole.WriteQuestion("Como uma Razor Page transforma rota, PageModel e layout em HTML final?");
        TutorialConsole.WriteHypothesis(
            "`AddRazorPages` registra os serviços necessários para localizar e renderizar páginas.",
            "`MapRazorPages` expõe os arquivos `.cshtml` como endpoints.",
            "O `PageModel` prepara dados e o layout define a estrutura comum do HTML.");
        TutorialConsole.WritePreparation(
            "`RazorPagesScenario` inicia um host local com Razor Pages compiladas no tutorial.",
            "`IndexModel` preenche dados exibidos pela página inicial.",
            "`_ViewStart` seleciona `_Layout`, e `_ViewImports` habilita o namespace e Tag Helpers.");

        TutorialConsole.WriteExperiment(
            1,
            "Serviços e endpoints",
            "O host registra Razor Pages e mapeia as páginas compiladas neste assembly.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: `AddRazorPages` e `MapRazorPages` conectam o runtime Razor ao endpoint routing.",
            typeof(RazorPagesScenario),
            nameof(RazorPagesScenario.CreateAppAsync));

        TutorialConsole.WriteExperiment(
            2,
            "PageModel e página",
            "A página `/` executa `IndexModel.OnGet` e renderiza os dados no arquivo `.cshtml`.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o PageModel prepara o texto antes da renderização.",
            typeof(Pages.IndexModel),
            nameof(Pages.IndexModel.OnGet));
        TutorialConsole.WriteCodeSnippet(
            "Código real: a página consome propriedades do PageModel.",
            "src/CSharpCodePortfolio.Tutorials.Tutorial23/Pages/Index.cshtml");

        var report = await new RazorPagesScenario().RunAsync(cancellationToken);
        VerifyReport(report);

        TutorialConsole.WriteEvidence(
            "HTML renderizado",
            ("Endereço", report.BaseAddress),
            ("Status", $"{(int)report.StatusCode} {report.StatusCode}"),
            ("Título", report.Title),
            ("Layout aplicado", YesNo(report.HasLayout)),
            ("Conteúdo do PageModel", YesNo(report.HasPageModelContent)));
        TutorialConsole.WriteEvidence(
            "Trechos encontrados",
            ("Navegação", YesNo(report.HasNavigation)),
            ("Página", YesNo(report.HasRazorPageMarker)),
            ("Privacidade", $"{(int)report.PrivacyStatusCode} {report.PrivacyStatusCode}"));

        TutorialConsole.WriteExperiment(
            3,
            "Teste automatizado",
            "O teste executa o host local e valida HTML, layout, PageModel e roteamento.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o teste prova que a página foi renderizada pelo runtime Razor.",
            "tests/CSharpCodePortfolio.Tutorials.Tutorial23.Tests/RazorPagesScenarioTests.cs");

        TutorialConsole.WriteObservation(
            "Razor Pages organiza telas simples por página: o `.cshtml` define marcação, o PageModel prepara dados e o layout evita duplicação de estrutura.");
        TutorialConsole.WriteConclusion(
            "O fluxo de Razor Pages fica claro quando a requisição é real: o endpoint local encontra a página, executa o PageModel, aplica o layout e devolve HTML verificável.",
            TutorialConclusionKind.Success);
    }

    private static void VerifyReport(RazorPageRenderReport report)
    {
        if (report.StatusCode != System.Net.HttpStatusCode.OK ||
            !report.HasLayout ||
            !report.HasPageModelContent ||
            !report.HasNavigation ||
            !report.HasRazorPageMarker)
        {
            throw new InvalidOperationException("A Razor Page não foi renderizada com o layout e o PageModel esperados.");
        }
    }

    private static string YesNo(bool value) => value ? "Sim" : "Não";
}
