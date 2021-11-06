using OpenIDAppRazor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIDAppRazor
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