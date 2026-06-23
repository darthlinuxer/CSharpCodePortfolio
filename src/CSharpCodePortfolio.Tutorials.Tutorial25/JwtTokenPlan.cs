namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal sealed record JwtTokenPlan(
    string Issuer,
    string Audience,
    TimeSpan Lifetime,
    TimeSpan ClockSkew,
    IReadOnlyList<string> Claims,
    IReadOnlyList<string> AuthenticationSchemes,
    IReadOnlyList<string> ExternalProviderSteps,
    IReadOnlyList<string> ValidationSteps);
