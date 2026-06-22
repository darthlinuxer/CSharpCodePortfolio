using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using System.Reflection;

namespace CSharpCodePortfolio.Tutorials.Tutorial07;

[Tutorial("07", "reflection-attributes", "Reflexão e atributos no lugar de if/else")]
public sealed class ReflectionAttributesTutorial : ITutorial
{
    private enum WorkflowState
    {
        Init,
        Running,
        Closed,
        Cancelled
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("07", "Reflexão e atributos no lugar de if/else");
        TutorialConsole.WriteContext(
            ("Tema", "Reflexão e atributos"),
            ("Conceito", "Atributos conectam estados a classes executoras"),
            ("Runtime", ".NET 10"),
            ("Slug", "reflection-attributes"));
        TutorialConsole.WriteQuestion("Como substituir uma decisão condicional por um catálogo descoberto via reflexão?");
        TutorialConsole.WriteHypothesis(
            "Cada classe de ação declara o estado que atende por meio de um atributo.",
            "A reflexão encontra a classe marcada para o estado recebido.",
            "Adicionar um estado novo passa a concentrar a mudança na nova ação, não em um bloco condicional maior.");
        TutorialConsole.WritePreparation(
            "O tutorial usa quatro estados de fluxo: início, execução, encerrado e cancelado.",
            "Primeiro, o resultado é calculado por um `switch` centralizado.",
            "Depois, o mesmo estado é resolvido por atributo e reflexão.");

        const WorkflowState state = WorkflowState.Running;

        TutorialConsole.WriteExperiment(
            1,
            "Decisão centralizada",
            "Mapeia cada estado para uma ação em um único `switch`.");
        TutorialConsole.WriteCodeSnippet(
            "O bloco precisa conhecer todos os estados aceitos.",
            "SwitchStateRouter.cs",
            """
            private static StateActionResult ExecuteWithSwitch(WorkflowState state) => state switch
            {
                WorkflowState.Init => new StateActionResult(1, "Preparar fluxo"),
                WorkflowState.Running => new StateActionResult(4, "Executar fluxo"),
                WorkflowState.Closed => new StateActionResult(3, "Encerrar fluxo"),
                WorkflowState.Cancelled => new StateActionResult(2, "Cancelar fluxo"),
                _ => throw new ArgumentOutOfRangeException(nameof(state))
            };
            """);

        var switchResult = ExecuteWithSwitch(state);
        TutorialConsole.WriteEvidence(
            "Switch",
            ("Estado", state.ToString()),
            ("Ação", switchResult.Description),
            ("Código", switchResult.Code.ToString()));

        TutorialConsole.WriteExperiment(
            2,
            "Catálogo por atributos",
            "Localiza a ação cujo atributo declara o estado solicitado.");
        TutorialConsole.WriteCodeSnippet(
            "A reflexão busca classes que implementam a ação e possuem o atributo do estado.",
            "AttributeStateRouter.cs",
            """
            var actionType = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(IsConcreteStateAction)
                .First(type => type.GetCustomAttribute<StateActionAttribute>()?.State == state);

            var action = (IStateAction)Activator.CreateInstance(actionType, nonPublic: true)!;
            """);

        var reflectionResult = ExecuteWithReflection(state);
        TutorialConsole.WriteEvidence(
            "Reflexão",
            ("Tipo encontrado", reflectionResult.ActionTypeName),
            ("Ação", reflectionResult.Result.Description),
            ("Código", reflectionResult.Result.Code.ToString()));

        TutorialConsole.WriteObservation(
            "Atributos funcionam como metadados de roteamento: eles tornam a associação explícita sem manter uma lista manual de casos.");
        TutorialConsole.WriteConclusion(
            "O contrato mínimo é um atributo com a chave de seleção, uma interface comum e uma busca por reflexão com validação.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static StateActionResult ExecuteWithSwitch(WorkflowState state) => state switch
    {
        WorkflowState.Init => new StateActionResult(1, "Preparar fluxo"),
        WorkflowState.Running => new StateActionResult(4, "Executar fluxo"),
        WorkflowState.Closed => new StateActionResult(3, "Encerrar fluxo"),
        WorkflowState.Cancelled => new StateActionResult(2, "Cancelar fluxo"),
        _ => throw new ArgumentOutOfRangeException(nameof(state))
    };

    private static ReflectionActionResult ExecuteWithReflection(WorkflowState state)
    {
        var actionType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(IsConcreteStateAction)
            .First(type => type.GetCustomAttribute<StateActionAttribute>()?.State == state);

        var action = (IStateAction)Activator.CreateInstance(actionType, nonPublic: true)!;
        return new ReflectionActionResult(actionType.Name, action.Execute());
    }

    private static bool IsConcreteStateAction(Type type) =>
        typeof(IStateAction).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract;

    [AttributeUsage(AttributeTargets.Class)]
    private sealed class StateActionAttribute(WorkflowState state) : Attribute
    {
        public WorkflowState State { get; } = state;
    }

    private interface IStateAction
    {
        StateActionResult Execute();
    }

    [StateAction(WorkflowState.Init)]
    private sealed class InitAction : IStateAction
    {
        public StateActionResult Execute() => new(1, "Preparar fluxo");
    }

    [StateAction(WorkflowState.Running)]
    private sealed class RunningAction : IStateAction
    {
        public StateActionResult Execute() => new(4, "Executar fluxo");
    }

    [StateAction(WorkflowState.Closed)]
    private sealed class ClosedAction : IStateAction
    {
        public StateActionResult Execute() => new(3, "Encerrar fluxo");
    }

    [StateAction(WorkflowState.Cancelled)]
    private sealed class CancelledAction : IStateAction
    {
        public StateActionResult Execute() => new(2, "Cancelar fluxo");
    }

    private sealed record StateActionResult(int Code, string Description);

    private sealed record ReflectionActionResult(string ActionTypeName, StateActionResult Result);
}
