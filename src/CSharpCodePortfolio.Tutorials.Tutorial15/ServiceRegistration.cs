using Microsoft.Extensions.DependencyInjection;

namespace CSharpCodePortfolio.Tutorials.Tutorial15;

internal static class ServiceRegistration
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IDemoService, DemoService>()
            .AddSingleton<ITestService, TestService>()
            .AddSingleton(new DefinedValueService(10))
            .AddTransient<TransientOperation>()
            .AddScoped<ScopedOperation>()
            .AddSingleton<SingletonOperation>()
            .AddTransient(typeof(GenericRepository<>))
            .AddSingleton<IMessageService, ServiceA>()
            .AddSingleton<IMessageService, ServiceB>();

        return services.BuildServiceProvider(validateScopes: true);
    }
}
