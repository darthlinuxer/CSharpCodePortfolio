using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace App.RequiredClaims
{
    public class CustomRequireClaim : IAuthorizationRequirement
    {
        public CustomRequireClaim(string claimType) => ClaimType = claimType;
        public CustomRequireClaim(string claimType, int value): this(claimType) => ClaimValue = value;

        public string ClaimType { get; init;}
        public int ClaimValue {get; init;} = 0;
    }

    public class CustomRequireClaimHandler : AuthorizationHandler<CustomRequireClaim>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRequireClaim requirement)
        {
            var userClaim = context.User.Claims.FirstOrDefault(x=>x.Type == requirement.ClaimType);
            if (userClaim is null) return Task.CompletedTask;
            if (userClaim.Type=="Security") if (Convert.ToInt32(userClaim.Value) >= requirement.ClaimValue) context.Succeed(requirement);
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

        public static AuthorizationPolicyBuilder RequireCustomClaim(this AuthorizationPolicyBuilder builder, string claimName, int claimValue)
        {
            builder.AddRequirements(new CustomRequireClaim(claimName, claimValue));
            return builder;
        }
    }
}