using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpCodePortfolio.Tutorials.Tutorial15;

[Tutorial("15", "dependency-injection-services", "Dependency Injection com Services")]
public sealed class DependencyInjectionServicesTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("15", "Dependency Injection com Services");
        TutorialConsole.WriteContext(
            ("Tema", "Microsoft.Extensions.DependencyInjection"),
            ("Conceito", "O container cria objetos, injeta dependências e controla ciclos de vida"),
            ("Runtime", ".NET 10"),
            ("Slug", "dependency-injection-services"));
        TutorialConsole.WriteQuestion("Como registrar e resolver serviços com `ServiceCollection` em uma aplicação console?");
        TutorialConsole.WriteHypothesis(
            "`AddSingleton`, `AddScoped` e `AddTransient` controlam quando uma instância é criada e reutilizada.",
            "Serviços podem receber dependências pelo construtor sem criar objetos manualmente.",
            "O container também resolve instâncias prontas, tipos genéricos abertos e múltiplas implementações.");
        TutorialConsole.WritePreparation(
            "O tutorial monta uma `ServiceCollection` sem `Host`, mantendo o fluxo visível.",
            "Dois escopos simulam duas operações independentes.",
            "Cada serviço expõe identificadores de instância para comparar os ciclos de vida.");

        TutorialConsole.WriteExperiment(
            1,
            "Registro dos serviços",
            "Configura ciclos de vida, serviço aninhado, instância pronta, genérico aberto e múltiplas implementações.");
        TutorialConsole.WriteCodeSnippet(
            "A coleção registra o contrato e o container resolve as dependências quando necessário.",
            typeof(ServiceRegistration),
            nameof(ServiceRegistration.Build),
            new CodeExcerpt(5, 14, "Registros da ServiceCollection"));

        TutorialConsole.WriteExperiment(
            2,
            "Comparação de ciclos de vida",
            "Resolve os mesmos serviços em dois escopos para comparar reaproveitamento e criação de instâncias.");
        TutorialConsole.WriteCodeSnippet(
            "Scoped e transient são resolvidos pelo escopo; singleton pode ser resolvido em qualquer escopo.",
            typeof(DependencyInjectionServicesTutorial),
            nameof(RunAsync),
            new CodeExcerpt(39, 51, "Dois escopos independentes"));

        using var serviceProvider = ServiceRegistration.Build();

        ServiceSnapshot first;
        using (var firstScope = serviceProvider.CreateScope())
        {
            first = ServiceSnapshot.Capture(firstScope.ServiceProvider);
        }

        ServiceSnapshot second;
        using (var secondScope = serviceProvider.CreateScope())
        {
            second = ServiceSnapshot.Capture(secondScope.ServiceProvider);
        }

        var testService = serviceProvider.GetRequiredService<ITestService>();
        var nestedMessages = testService.RunDemo();
        var configuredValue = serviceProvider.GetRequiredService<DefinedValueService>().Value;
        var repository = serviceProvider.GetRequiredService<GenericRepository<Client>>();
        var repositoryMessage = repository.Add(new Client("Camilo"));
        var messages = serviceProvider
            .GetServices<IMessageService>()
            .Select(static service => service.SaySomething())
            .ToArray();
        var defaultMessage = serviceProvider.GetRequiredService<IMessageService>().SaySomething();

        VerifyScenario(first, second, nestedMessages, configuredValue, messages, defaultMessage);

        TutorialConsole.WriteEvidence(
            "Ciclos de vida",
            ("Singleton", $"{first.SingletonId} | {second.SingletonId}"),
            ("Scoped", $"{first.ScopedId} | {second.ScopedId}"),
            ("Transient", $"{first.TransientId} | {second.TransientId}"),
            ("Descarte do escopo", FormatDisposal(first)),
            ("Serviço aninhado", string.Join(" | ", nestedMessages)),
            ("Instância pronta", configuredValue.ToString()),
            ("Genérico aberto", repositoryMessage),
            ("Múltiplas implementações", string.Join(" | ", messages)),
            ("Resolução padrão", defaultMessage));

        TutorialConsole.WriteObservation(
            "O singleton mantém a mesma instância entre escopos; o scoped muda por escopo; o transient muda a cada resolução.");
        TutorialConsole.WriteConclusion(
            "`ServiceCollection` centraliza a composição da aplicação e mantém o código de uso dependente de contratos e ciclos de vida explícitos.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static void VerifyScenario(
        ServiceSnapshot first,
        ServiceSnapshot second,
        IReadOnlyList<string> nestedMessages,
        int configuredValue,
        IReadOnlyList<string> messages,
        string defaultMessage)
    {
        if (first.SingletonId != second.SingletonId)
        {
            throw new InvalidOperationException("O singleton deve manter a mesma instância entre escopos.");
        }

        if (first.ScopedId == second.ScopedId)
        {
            throw new InvalidOperationException("O scoped deve criar uma instância por escopo.");
        }

        if (first.TransientId == second.TransientId)
        {
            throw new InvalidOperationException("O transient deve criar novas instâncias.");
        }

        if (!first.ScopedDisposed || !first.TransientDisposed || first.SingletonDisposed)
        {
            throw new InvalidOperationException("Scoped e transient devem ser descartados ao final do escopo.");
        }

        if (nestedMessages.Count != 2 || configuredValue != 10)
        {
            throw new InvalidOperationException("Serviço aninhado e instância pronta devem ser resolvidos pelo container.");
        }

        if (messages is not ["I am Service A", "I am Service B"] || defaultMessage != "I am Service B")
        {
            throw new InvalidOperationException("Múltiplas implementações devem ser resolvidas em ordem de registro.");
        }
    }

    private static string FormatDisposal(ServiceSnapshot snapshot) =>
        $"scoped: {FormatDisposed(snapshot.ScopedDisposed)} | transient: {FormatDisposed(snapshot.TransientDisposed)}";

    private static string FormatDisposed(bool disposed) =>
        disposed ? "descartado" : "ativo";
}
