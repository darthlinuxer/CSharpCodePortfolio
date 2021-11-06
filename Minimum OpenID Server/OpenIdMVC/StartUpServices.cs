using OpenIDAppMVC.Services;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIDAppMVC
{
    public class StartUpServices
    {
        public static void Init(IServiceCollection services)
        {
            services.AddScoped<SecretService>();
            services.AddScoped<TokenTools>();
        }
    }
}