namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record AuthorizationCodeGrant(
    string Code,
    string ClientId,
    string RedirectUri,
    string Subject,
    string Scope,
    string CodeChallenge,
    DateTimeOffset ExpiresAt);
