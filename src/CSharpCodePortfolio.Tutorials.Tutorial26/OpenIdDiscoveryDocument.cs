namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record OpenIdDiscoveryDocument(
    string Issuer,
    string AuthorizationEndpoint,
    string TokenEndpoint,
    string UserInfoEndpoint,
    string JwksUri,
    IReadOnlyList<string> ResponseTypesSupported,
    IReadOnlyList<string> SubjectTypesSupported,
    IReadOnlyList<string> IdTokenSigningAlgValuesSupported,
    IReadOnlyList<string> ScopesSupported)
{
    public static OpenIdDiscoveryDocument Create(string issuer)
    {
        var normalizedIssuer = issuer.TrimEnd('/');
        return new OpenIdDiscoveryDocument(
            normalizedIssuer,
            $"{normalizedIssuer}/connect/authorize",
            $"{normalizedIssuer}/connect/token",
            $"{normalizedIssuer}/connect/userinfo",
            $"{normalizedIssuer}/.well-known/jwks.json",
            ["code"],
            ["public"],
            ["HS256"],
            ["openid", "profile", "email"]);
    }
}
