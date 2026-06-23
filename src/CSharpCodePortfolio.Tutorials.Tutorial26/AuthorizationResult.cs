namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record AuthorizationResult(
    string RedirectUri,
    string Code,
    string State);
