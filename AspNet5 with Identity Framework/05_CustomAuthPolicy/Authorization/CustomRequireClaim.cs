using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using crypto;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace App.RequiredClaims
{
    public class CustomRequireClaim : IAuthorizationRequirement
    {
        public CustomRequireClaim(string claimType) => ClaimType = claimType;
        public CustomRequireClaim(string claimType, string value): this(claimType) => ClaimValue = value;

        public string ClaimType { get; init;}
        public string ClaimValue {get; init;} 
    }

    public class CustomRequireClaimHandler : AuthorizationHandler<CustomRequireClaim>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRequireClaim requirement)
        {
            var userClaim = context.User.Claims.FirstOrDefault(x=>x.Type == requirement.ClaimType);
            if (userClaim is null) return Task.CompletedTask;

            switch(requirement.ClaimType)
            {
                case "Security": 
                {
                    if(Convert.ToInt32(userClaim.Value) >= Convert.ToInt32(requirement.ClaimValue)) 
                    context.Succeed(requirement);
                    break;
                }
                default:
                {
                    if(requirement.ClaimValue is not null)
                    {
                        if(requirement.ClaimValue== userClaim.Value) context.Succeed(requirement);
                    } else context.Succeed(requirement);
                    break;
                }     
            }
            return Task.CompletedTask;
        }
    }

    public static class AuthorizationPolicyBuilderExtension
    {
        public static AuthorizationPolicyBuilder RequireCustomClaim(this AuthorizationPolicyBuilder builder, string claimName)
        {
            builder.AddRequirements(new CustomRequireClaim(claimName));
            return builder;
        }

        public static AuthorizationPolicyBuilder RequireCustomClaim(this AuthorizationPolicyBuilder builder, string claimName, string claimValue)
        {
            builder.AddRequirements(new CustomRequireClaim(claimName, claimValue));
            return builder;
        }
    }
}