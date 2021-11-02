using OAuthApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace OAuthApp
{
    public class StartUpServices
    {
        public static void Init(IServiceCollection services)
        {
            services.AddScoped<TokenTools>();
            services.AddScoped<SignInManager<IdentityUser>>();
            services.AddScoped<UserManager<IdentityUser>>();
        }
    }
}