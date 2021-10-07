using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using App.RequiredClaims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace App.PolicyProvider
{
    public static class CustomAuthorizationPolicyFactory
    {
        public static AuthorizationPolicy Create(string policyName)
        {
            var parts = policyName.Split('.');
            var type = parts.First();
            var value = parts.Last();
            return new AuthorizationPolicyBuilder().AddRequirements(new CustomRequireClaim(type,Convert.ToInt32(value))).Build();
        }
    }

    public class CustomAuthPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public CustomAuthPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
            options.Value.AddPolicy("UserPolicy", policyBuilder =>
              {
                  policyBuilder.RequireAuthenticatedUser();
                  policyBuilder.RequireClaim(ClaimTypes.Role,"User");
                  //policyBuilder.AddRequirements(new CustomRequireClaim("Email"));
                  policyBuilder.RequireCustomClaim("Email");              
                  //policyBuilder.RequireCustomClaim("Age");
              });
        }

        //PolicyName.Value
        public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {            
            if(policyName.StartsWith("Security"))
            {
                var policy = CustomAuthorizationPolicyFactory.Create(policyName);
                return Task.FromResult(policy);
            }            
            return base.GetPolicyAsync(policyName);
        }
      
        
    }
        
 
}