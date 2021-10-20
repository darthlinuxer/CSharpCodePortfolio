using App.PolicyProvider;
using App.RequiredClaims;
using App.TokenLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public class StartUpServices
    {
        public static void Init(IServiceCollection services)
        {
            services.AddScoped<IAuthorizationHandler, CustomRequireClaimHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthPolicyProvider>();
            services.AddScoped<TokenTools>();
            services.AddScoped<SignInManager<IdentityUser>>();
            services.AddScoped<UserManager<IdentityUser>>();
        }
    }
}