using OAuthApp.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OAuthApp
{
    public class StartupDbContext
    {
        public static void Init(IServiceCollection services)
        {

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("MemoryDb");
            });

            services.AddDbContext<OAuthDbContext>(options =>
            {
                options.UseInMemoryDatabase("OAuthDb");
            });         

        }
    }
}