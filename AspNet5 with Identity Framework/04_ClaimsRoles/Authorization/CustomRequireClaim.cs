using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Crypto.Engines;

namespace App.RequiredClaims
{
    public class CustomRequireClaim : IAuthorizationRequirement
    {
        public CustomRequireClaim(string claimType)
        {
            ClaimType = claimType;
        }

        public string ClaimType { get; }
    }

    public class CustomRequireClaimHandler : AuthorizationHandler<CustomRequireClaim>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRequireClaim requirement)
        {
            var hasClaim = context.User.Claims.Any(x=>x.Type == requirement.ClaimType);
            if(hasClaim) context.Succeed(requirement);
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
    }
}