namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal static class MinimumOpenIdServerScenario
{
    public static MinimumOpenIdServerReport Run()
    {
        var server = CreateServer();
        var discovery = server.GetDiscoveryDocument();
        var verifier = "portfolio-verifier-12345";
        var authorization = server.Authorize(new AuthorizationRequest(
            ClientId: "portfolio-console",
            RedirectUri: "https://client.local/callback",
            Scope: "openid profile email",
            ResponseType: "code",
            State: "state-123",
            CodeChallenge: Pkce.CreateChallenge(verifier),
            CodeChallengeMethod: "S256"));
        var token = server.Exchange(new TokenRequest(
            ClientId: "portfolio-console",
            ClientSecret: "dev-secret",
            Code: authorization.Code,
            RedirectUri: "https://client.local/callback",
            CodeVerifier: verifier));
        var idTokenValidation = server.ValidateToken(token.IdToken, "id_token");
        var accessTokenValidation = server.ValidateToken(token.AccessToken, "access_token");
        var userInfo = server.GetUserInfo(token.AccessToken);

        var authorizationCodeReuseBlocked = ThrowsInvalidOperation(() => server.Exchange(new TokenRequest(
            ClientId: "portfolio-console",
            ClientSecret: "dev-secret",
            Code: authorization.Code,
            RedirectUri: "https://client.local/callback",
            CodeVerifier: verifier)));
        var tamperedAccessTokenBlocked = !server.ValidateToken($"{token.AccessToken}x", "access_token").IsValid;

        return new MinimumOpenIdServerReport(
            discovery,
            authorization,
            token,
            idTokenValidation,
            accessTokenValidation,
            userInfo,
            authorizationCodeReuseBlocked,
            tamperedAccessTokenBlocked,
            FlowSteps:
            [
                "Publicar discovery com issuer, authorization_endpoint, token_endpoint e userinfo_endpoint.",
                "Validar client_id, redirect_uri, response_type=code, escopo openid e PKCE.",
                "Redirecionar o cliente com authorization code e state preservado.",
                "Trocar code por id_token e access_token somente com client_secret e code_verifier corretos.",
                "Bloquear reutilização do authorization code.",
                "Validar issuer, assinatura, expiração e token_use.",
                "Expor userinfo apenas com access_token válido."
            ]);
    }

    public static MinimumOpenIdServer CreateServer(Func<DateTimeOffset>? getNow = null)
    {
        return new MinimumOpenIdServer(
            OpenIdServerOptions.TutorialDefaults(getNow),
            new OpenIdClient(
                ClientId: "portfolio-console",
                ClientSecret: "dev-secret",
                RedirectUri: "https://client.local/callback"),
            new OpenIdUser(
                Subject: "user-001",
                Email: "ana.admin@portfolio.test",
                Name: "Ana Admin"));
    }

    private static bool ThrowsInvalidOperation(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }
}
