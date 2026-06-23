using Microsoft.Extensions.DependencyInjection;

namespace CSharpCodePortfolio.Tutorials.Tutorial16;

internal static class ServiceRegistration
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();

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

        return services.BuildServiceProvider(validateScopes: true);
    }
}
