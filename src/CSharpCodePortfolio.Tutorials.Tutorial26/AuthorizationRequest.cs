namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record AuthorizationRequest(
    string ClientId,
    string RedirectUri,
    string Scope,
    string ResponseType,
    string State,
    string CodeChallenge,
    string CodeChallengeMethod);
