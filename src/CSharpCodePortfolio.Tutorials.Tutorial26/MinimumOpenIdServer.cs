using System.Security.Cryptography;

namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed class MinimumOpenIdServer
{
    private readonly Dictionary<string, AuthorizationCodeGrant> authorizationCodes = new(StringComparer.Ordinal);
    private readonly OpenIdClient client;
    private readonly OpenIdServerOptions options;
    private readonly HmacTokenService tokenService;
    private readonly OpenIdUser user;

    public MinimumOpenIdServer(
        OpenIdServerOptions options,
        OpenIdClient client,
        OpenIdUser user)
    {
        this.options = options;
        this.client = client;
        this.user = user;
        tokenService = new HmacTokenService(options);
    }

    public OpenIdDiscoveryDocument GetDiscoveryDocument()
    {
        return OpenIdDiscoveryDocument.Create(options.Issuer);
    }

    public AuthorizationResult Authorize(AuthorizationRequest request)
    {
        if (request.ClientId != client.ClientId)
        {
            throw new InvalidOperationException("Cliente não registrado.");
        }

        if (request.RedirectUri != client.RedirectUri)
        {
            throw new InvalidOperationException("Redirect URI não registrado para este cliente.");
        }

        if (!string.Equals(request.ResponseType, "code", StringComparison.Ordinal)
            || !request.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("openid"))
        {
            throw new InvalidOperationException("A autorização OpenID mínima exige response_type=code e escopo openid.");
        }

        if (request.CodeChallengeMethod != "S256" || string.IsNullOrWhiteSpace(request.CodeChallenge))
        {
            throw new InvalidOperationException("PKCE S256 é obrigatório para emitir authorization code.");
        }

        var code = Base64Url.Encode(RandomNumberGenerator.GetBytes(32));
        authorizationCodes[code] = new AuthorizationCodeGrant(
            code,
            request.ClientId,
            request.RedirectUri,
            user.Subject,
            request.Scope,
            request.CodeChallenge,
            options.GetNow().Add(options.CodeLifetime));

        return new AuthorizationResult(
            BuildRedirectUri(request.RedirectUri, code, request.State),
            code,
            request.State);
    }

    public TokenResponse Exchange(TokenRequest request)
    {
        if (request.ClientId != client.ClientId || request.ClientSecret != client.ClientSecret)
        {
            throw new InvalidOperationException("Credenciais do cliente inválidas.");
        }

        if (!authorizationCodes.Remove(request.Code, out var grant))
        {
            throw new InvalidOperationException("Authorization code inválido ou já utilizado.");
        }

        if (grant.ExpiresAt <= options.GetNow() || grant.RedirectUri != request.RedirectUri)
        {
            throw new InvalidOperationException("Authorization code expirado ou emitido para outro redirect URI.");
        }

        if (Pkce.CreateChallenge(request.CodeVerifier) != grant.CodeChallenge)
        {
            throw new InvalidOperationException("Code verifier não corresponde ao code challenge.");
        }

        var commonClaims = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["email"] = user.Email,
            ["name"] = user.Name
        };

        return new TokenResponse(
            TokenType: "Bearer",
            Scope: grant.Scope,
            ExpiresIn: (int)options.TokenLifetime.TotalSeconds,
            IdToken: tokenService.CreateToken(user.Subject, client.ClientId, "id_token", grant.Scope, commonClaims),
            AccessToken: tokenService.CreateToken(user.Subject, "portfolio-api", "access_token", grant.Scope, commonClaims));
    }

    public UserInfoResponse GetUserInfo(string accessToken)
    {
        var validation = ValidateToken(accessToken, "access_token");
        if (!validation.IsValid || validation.Subject != user.Subject)
        {
            throw new InvalidOperationException("Access token inválido para userinfo.");
        }

        return new UserInfoResponse(user.Subject, user.Email, user.Name);
    }

    public TokenValidationResult ValidateToken(string token, string expectedTokenUse)
    {
        return tokenService.Validate(token, expectedTokenUse);
    }

    private static string BuildRedirectUri(string redirectUri, string code, string state)
    {
        var separator = redirectUri.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return string.Concat(
            redirectUri,
            separator,
            "code=",
            Uri.EscapeDataString(code),
            "&state=",
            Uri.EscapeDataString(state));
    }
}
