namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record MinimumOpenIdServerReport(
    OpenIdDiscoveryDocument Discovery,
    AuthorizationResult Authorization,
    TokenResponse Token,
    TokenValidationResult IdTokenValidation,
    TokenValidationResult AccessTokenValidation,
    UserInfoResponse UserInfo,
    bool AuthorizationCodeReuseBlocked,
    bool TamperedAccessTokenBlocked,
    IReadOnlyList<string> FlowSteps);
