namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal sealed record IdentityClaimsReport(
    string CookieAuthenticationType,
    string BearerAuthenticationType,
    bool HasRole,
    bool HasEmailClaim,
    bool HasSecurityLevelClaim,
    int UserPolicyRequirementCount,
    int SecurityPolicyRequirementCount,
    bool PrincipalMustBeRenewedAfterClaimChanges);
