using OpenIDApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIDApp
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