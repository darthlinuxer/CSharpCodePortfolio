using OpenIDApp.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIDApp
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