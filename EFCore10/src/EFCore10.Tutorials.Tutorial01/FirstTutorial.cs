using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;

namespace EFCore10.Tutorials.Tutorial01;

[Tutorial("01", "simple-modeling", "Modelagem simples com EF Core")]
public sealed class FirstTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("01", "Modelagem simples com EF Core");
        TutorialConsole.WriteContext(
            ("Provider", "SQLite"),
            ("Configuração", "OnConfiguring no DbContext"),
            ("Schema", "Migration InitialCreate"));

        TutorialConsole.WriteQuestion(
            "Qual é o fluxo mínimo para modelar Blog/Post e executar CRUD com EF Core?");
        TutorialConsole.WriteHypothesis(
            "Com modelos CLR, DbContext e migration aplicada, o EF Core consegue mapear tabelas e persistir objetos.",
            "O Change Tracker detecta alterações no blog e nos posts durante a unidade de trabalho.");

        await CRUD.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
