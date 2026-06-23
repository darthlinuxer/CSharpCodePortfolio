namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record OpenIdClient(
    string ClientId,
    string ClientSecret,
    string RedirectUri);
