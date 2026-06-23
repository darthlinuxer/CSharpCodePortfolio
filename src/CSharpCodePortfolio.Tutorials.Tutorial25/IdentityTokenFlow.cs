using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal static class IdentityTokenFlow
{
    public static JwtTokenPlan CreateTokenPlan(PortfolioIdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var principal = IdentityClaimsFlow.BuildPrincipal(user, "Bearer");
        var claims = principal.Claims
            .Select(DescribeClaim)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new JwtTokenPlan(
            Issuer: "CSharpCodePortfolio",
            Audience: "portfolio-api",
            Lifetime: TimeSpan.FromHours(8),
            ClockSkew: TimeSpan.Zero,
            Claims: claims,
            AuthenticationSchemes:
            [
                $"{IdentityConstants.ApplicationScheme}: cookie de sessão para páginas e endpoints interativos.",
                "Bearer: access_token JWT para APIs.",
                "External: OAuth/OpenID Connect para redirecionamento e callback de provedores."
            ],
            ExternalProviderSteps:
            [
                "Redirecionar para o endpoint de autorização do provedor com state e PKCE.",
                "Receber o callback, validar state e trocar code por tokens no backchannel.",
                "Ler id_token/userinfo do OpenID Connect, mapear claims e vincular login externo ao usuário local.",
                "Em provedor próprio, publicar discovery, authorize, token e userinfo com escopos controlados."
            ],
            ValidationSteps:
            [
                "Validar assinatura, issuer, audience e expiração.",
                "Usar access_token para APIs; id_token descreve autenticação do cliente em OpenID Connect.",
                "Recarregar claims no OnTokenValidated quando roles ou claims puderem mudar depois da emissão."
            ]);
    }

    private static string DescribeClaim(Claim claim)
    {
        return claim.Type switch
        {
            ClaimTypes.NameIdentifier => $"sub={claim.Value}",
            ClaimTypes.Email => $"email={claim.Value}",
            ClaimTypes.Role => $"role={claim.Value}",
            _ => $"{claim.Type}={claim.Value}"
        };
    }
}
