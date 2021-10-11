using System.Security.Claims;
using App.PolicyProvider;
using App.RequiredClaims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public class StartupPolicies
    {
       public static void Init(IServiceCollection services)
       {          
          services.AddScoped<IAuthorizationHandler, CustomRequireClaimHandler>();
          services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthPolicyProvider>();
       }
    }
}