using OpenIDApp.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIDApp
{
    public class StartupDbContext
    {
        public static void Init(IServiceCollection services)
        {

            services.AddDbContext<OpenIdDbContext>(options =>
            {
                options.UseInMemoryDatabase("OpenIdDb");
            });         

        }
    }
}