namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record TokenRequest(
    string ClientId,
    string ClientSecret,
    string Code,
    string RedirectUri,
    string CodeVerifier);
