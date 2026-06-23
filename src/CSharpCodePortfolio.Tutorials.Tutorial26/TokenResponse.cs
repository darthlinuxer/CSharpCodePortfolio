namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record TokenResponse(
    string TokenType,
    string Scope,
    int ExpiresIn,
    string IdToken,
    string AccessToken);
