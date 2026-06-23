using System.Globalization;
using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpCodePortfolio.Tutorials.Tutorial16;

[Tutorial("16", "dependency-inversion-services", "Inversão de Dependência com Injeção por Construtor e por Propriedade")]
public sealed class DependencyInversionServicesTutorial : ITutorial
{
    private static readonly CultureInfo BrazilianPortuguese = CultureInfo.GetCultureInfo("pt-BR");

    public Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("16", "Inversão de Dependência com Injeção por Construtor e por Propriedade");
        TutorialConsole.WriteContext(
            ("Tema", "Princípio da Inversão de Dependência"),
            ("Conceito", "Serviços de alto nível dependem de contratos, não de implementações concretas"),
            ("Runtime", ".NET 10"),
            ("Slug", "dependency-inversion-services"));
        TutorialConsole.WriteQuestion("Como aplicar inversão de dependência com injeção por construtor e por propriedade?");
        TutorialConsole.WriteHypothesis(
            "`ISalaryCalculator` isola a regra de cálculo da folha de pagamento.",
            "A injeção por construtor deixa dependências obrigatórias explícitas no construtor.",
            "A injeção por propriedade atende dependências configuradas por uma função de criação quando o ponto de composição precisa montar o objeto.");
        TutorialConsole.WritePreparation(
            "O contêiner registra `ISalaryCalculator` e duas formas de montar o serviço de folha.",
            "O cenário calcula salários com as mesmas regras e valores diferentes.",
            "As evidências mostram contrato usado, total calculado e tipo concreto resolvido pelo contêiner.");

        TutorialConsole.WriteExperiment(
            1,
            "Registro no contêiner",
            "Registra o contrato de cálculo e duas classes de folha de pagamento.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o ponto de composição decide qual implementação atende o contrato.",
            "Registro.cs",
            """
            services
                .AddTransient<ISalaryCalculator, SalaryCalculator>()
                .AddTransient<ConstructorInjectedPayroll>()
                .AddScoped(provider =>
                {
                    var calculator = provider.GetRequiredService<ISalaryCalculator>();
                    return new PropertyInjectedPayroll
                    {
                        SalaryCalculator = calculator
                    };
                });
            """);

        TutorialConsole.WriteExperiment(
            2,
            "Injeção por construtor",
            "Resolve um serviço que recebe a dependência obrigatória pelo construtor.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: `ConstructorInjectedPayroll` não cria a calculadora; ele recebe `ISalaryCalculator`.",
            "Construtor.cs",
            """
            internal sealed class ConstructorInjectedPayroll(ISalaryCalculator salaryCalculator)
            {
                public decimal Calculate(SalaryInput input) =>
                    salaryCalculator.Calculate(input);
            }
            """);

        TutorialConsole.WriteExperiment(
            3,
            "Injeção por propriedade",
            "Resolve um serviço montado por uma função de criação, com a dependência atribuída por propriedade.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a propriedade é preenchida no registro e validada antes do uso.",
            "Propriedade.cs",
            """
            internal sealed class PropertyInjectedPayroll
            {
                public required ISalaryCalculator SalaryCalculator
                {
                    private get;
                    init;
                }

                public decimal Calculate(SalaryInput input) =>
                    SalaryCalculator.Calculate(input);
            }
            """);

        using var serviceProvider = ServiceRegistration.Build();
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var constructorPayroll = services.GetRequiredService<ConstructorInjectedPayroll>();
        var propertyPayroll = services.GetRequiredService<PropertyInjectedPayroll>();

        var constructorInput = new SalaryInput(HoursWorked: 150, HourlyRate: 50m);
        var propertyInput = new SalaryInput(HoursWorked: 150, HourlyRate: 100m);
        var report = new PayrollReport(
            ConstructorSalary: constructorPayroll.Calculate(constructorInput),
            PropertySalary: propertyPayroll.Calculate(propertyInput),
            ConstructorCalculator: constructorPayroll.CalculatorName,
            PropertyCalculator: propertyPayroll.CalculatorName);

        VerifyReport(report);

        TutorialConsole.WriteEvidence(
            "Inversão de dependência",
            ("Contrato", nameof(ISalaryCalculator)),
            ("Injeção por construtor", $"{report.ConstructorCalculator} -> {FormatMoney(report.ConstructorSalary)}"),
            ("Injeção por propriedade", $"{report.PropertyCalculator} -> {FormatMoney(report.PropertySalary)}"),
            ("Horas base", $"{constructorInput.HoursWorked}h e {propertyInput.HoursWorked}h"),
            ("Taxas", $"{FormatMoney(constructorInput.HourlyRate)}/h e {FormatMoney(propertyInput.HourlyRate)}/h"));
        TutorialConsole.WriteObservation(
            "A injeção por construtor é a opção padrão para dependências obrigatórias; a injeção por propriedade fica explícita no ponto de composição.");
        TutorialConsole.WriteConclusion(
            "A inversão de dependência deixa a regra de folha depender de `ISalaryCalculator`, permitindo trocar a implementação sem reescrever o fluxo de pagamento.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static void VerifyReport(PayrollReport report)
    {
        if (report.ConstructorSalary != 7500m)
        {
            throw new InvalidOperationException("O cálculo por injeção por construtor deve retornar 7500.");
        }

        if (report.PropertySalary != 15000m)
        {
            throw new InvalidOperationException("O cálculo por injeção por propriedade deve retornar 15000.");
        }

        if (report.ConstructorCalculator != nameof(SalaryCalculator) || report.PropertyCalculator != nameof(SalaryCalculator))
        {
            throw new InvalidOperationException("Os dois fluxos devem usar a implementação registrada para ISalaryCalculator.");
        }
    }

    private static string FormatMoney(decimal value) =>
        value.ToString("C", BrazilianPortuguese);
}
