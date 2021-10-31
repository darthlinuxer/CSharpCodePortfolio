using App.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public class StartupDbCultures
    {
        public static void Init(IServiceCollection services)
        {

            services.AddDbContext<CulturesDbContext>(options =>
            {
                options.UseInMemoryDatabase("CulturesDb");
            });         

        }
    }
}