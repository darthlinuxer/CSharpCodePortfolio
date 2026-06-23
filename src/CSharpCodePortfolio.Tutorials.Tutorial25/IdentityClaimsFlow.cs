using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal static class IdentityClaimsFlow
{
    public static ClaimsPrincipal BuildPrincipal(PortfolioIdentityUser user, string authenticationType)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(authenticationType);

        var identity = new ClaimsIdentity(authenticationType);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));

        foreach (var role in user.Roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        foreach (var claim in user.Claims)
        {
            identity.AddClaim(claim);
        }

        return new ClaimsPrincipal(identity);
    }

    public static AuthorizationPolicy CreateUserPolicy()
    {
        return new AuthorizationPolicyBuilder(IdentityConstants.ApplicationScheme, "Bearer")
            .RequireAuthenticatedUser()
            .RequireRole("User")
            .RequireClaim("Email")
            .Build();
    }

    public static AuthorizationPolicy CreateSecurityLevelPolicy(int requiredLevel)
    {
        if (requiredLevel <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredLevel), requiredLevel, "Security level must be greater than zero.");
        }

        return new AuthorizationPolicyBuilder("Bearer")
            .RequireAuthenticatedUser()
            .RequireClaim("SecurityLevel", requiredLevel.ToString())
            .Build();
    }

    public static IdentityClaimsReport Analyze(PortfolioIdentityUser user)
    {
        var cookiePrincipal = BuildPrincipal(user, IdentityConstants.ApplicationScheme);
        var bearerPrincipal = BuildPrincipal(user, "Bearer");
        var userPolicy = CreateUserPolicy();
        var securityPolicy = CreateSecurityLevelPolicy(10);

        return new IdentityClaimsReport(
            cookiePrincipal.Identity?.AuthenticationType ?? string.Empty,
            bearerPrincipal.Identity?.AuthenticationType ?? string.Empty,
            cookiePrincipal.IsInRole("User"),
            cookiePrincipal.HasClaim("Email", user.Email),
            bearerPrincipal.HasClaim("SecurityLevel", "10"),
            userPolicy.Requirements.Count,
            securityPolicy.Requirements.Count,
            PrincipalMustBeRenewedAfterClaimChanges: true);
    }
}
