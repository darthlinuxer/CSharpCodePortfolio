using System.Security.Claims;
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
          services.AddAuthorization(config =>
          {
              config.AddPolicy("UserPolicy", policyBuilder =>
              {
                  policyBuilder.RequireAuthenticatedUser();
                  policyBuilder.RequireClaim(ClaimTypes.Role,"User");
                  //policyBuilder.AddRequirements(new CustomRequireClaim("Email"));
                  policyBuilder.RequireCustomClaim("Email");              
                  //policyBuilder.RequireCustomClaim("Age");
              });
          });             

       }
    }
}