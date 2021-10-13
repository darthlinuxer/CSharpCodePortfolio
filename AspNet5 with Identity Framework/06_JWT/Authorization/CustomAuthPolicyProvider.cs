using System.Linq;
using System.Threading.Tasks;
using App.RequiredClaims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
            if(!policyName.Contains('.')) 
            return new AuthorizationPolicyBuilder().AddRequirements(new CustomRequireClaim(type)).Build();
            //if(!policyName.Contains('.')) return new AuthorizationPolicyBuilder().RequireClaim(type).Build();
            return new AuthorizationPolicyBuilder().RequireCustomClaim(type,value).Build();
        }
    }

    public class CustomAuthPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public CustomAuthPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {            
            options.Value.DefaultPolicy = new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();

            options.Value.AddPolicy("Bearer", policyBuilder => policyBuilder
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                );  

            options.Value.AddPolicy("IdentityCookie", policyBuilder => policyBuilder
                .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                .RequireAuthenticatedUser()
                );          
        }

        //PolicyName.Value
        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {       

            if(policyName == "Bearer" || policyName == "IdentityCookie"){
            var bearerPolicy = await base.GetPolicyAsync(policyName);
            return bearerPolicy;
            }     
            
            var policy = CustomAuthorizationPolicyFactory.Create(policyName);
            return policy;
        }        
    } 
}