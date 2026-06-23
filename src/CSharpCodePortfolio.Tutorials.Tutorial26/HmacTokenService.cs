using System.Security.Cryptography;
using System.Text;

namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed class HmacTokenService(OpenIdServerOptions options)
{
    public string CreateToken(
        string subject,
        string audience,
        string tokenUse,
        string scope,
        IReadOnlyDictionary<string, string> extraClaims)
    {
        var now = options.GetNow();
        var claims = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["aud"] = audience,
            ["exp"] = now.Add(options.TokenLifetime).ToUnixTimeSeconds().ToString(),
            ["iat"] = now.ToUnixTimeSeconds().ToString(),
            ["iss"] = options.Issuer,
            ["scope"] = scope,
            ["sub"] = subject,
            ["token_use"] = tokenUse
        };

        foreach (var (key, value) in extraClaims)
        {
            claims[key] = value;
        }

        var header = Base64Url.EncodeJson(new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        });
        var payload = Base64Url.EncodeJson(claims);
        var unsignedToken = $"{header}.{payload}";

        return $"{unsignedToken}.{Sign(unsignedToken)}";
    }

    public TokenValidationResult Validate(string token, string expectedTokenUse)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return TokenValidationResult.Fail("Token deve conter header, payload e assinatura.");
        }

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(Sign(unsignedToken)),
                Encoding.ASCII.GetBytes(parts[2])))
        {
            return TokenValidationResult.Fail("Assinatura inválida.");
        }

        var claims = Base64Url.DecodeJson(parts[1]);
        if (!claims.TryGetValue("iss", out var issuer) || issuer != options.Issuer)
        {
            return TokenValidationResult.Fail("Issuer inválido.");
        }

        if (!claims.TryGetValue("token_use", out var tokenUse) || tokenUse != expectedTokenUse)
        {
            return TokenValidationResult.Fail("Tipo de token inválido.");
        }

        if (!claims.TryGetValue("exp", out var expText)
            || !long.TryParse(expText, out var exp)
            || DateTimeOffset.FromUnixTimeSeconds(exp) <= options.GetNow())
        {
            return TokenValidationResult.Fail("Token expirado.");
        }

        return TokenValidationResult.Success(claims);
    }

    private string Sign(string unsignedToken)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.SigningKey));
        return Base64Url.Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)));
    }
}
