using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpCodePortfolio.Tutorials.Tutorial13;

internal static class SchoolServiceRegistration
{
    public static ServiceProvider Build(string databaseName)
    {
        var services = new ServiceCollection();

        services.AddDbContext<SchoolDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddScoped<SchoolService>();

        return services.BuildServiceProvider();
    }
}
