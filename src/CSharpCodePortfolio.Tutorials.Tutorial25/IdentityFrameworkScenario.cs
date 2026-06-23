using Microsoft.AspNetCore.Identity;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal static class IdentityFrameworkScenario
{
    public static IdentityFrameworkReport Run()
    {
        var passwordFlow = new IdentityPasswordFlow();
        var registration = passwordFlow.Register(
            new IdentityRegistrationRequest("ana.admin@portfolio.test", "senha-segura"));

        var user = registration.User;
        var claims = IdentityClaimsFlow.Analyze(user);
        var token = IdentityTokenFlow.CreateTokenPlan(user);

        return new IdentityFrameworkReport(
            user.Email,
            PasswordHashLooksProtected: user.PasswordHash is not null
                && !user.PasswordHash.Contains("senha-segura", StringComparison.Ordinal),
            registration.CanSignInBeforeEmailConfirmation,
            registration.CanSignInAfterEmailConfirmation,
            registration.EmailConfirmationSucceeded,
            registration.CorrectPasswordResult,
            registration.WrongPasswordResult,
            claims,
            token,
            FlowSteps:
            [
                "Registrar usuário com hash de senha.",
                "Gerar token de confirmação de e-mail.",
                "Bloquear login enquanto a conta não está confirmada.",
                "Confirmar e-mail com token ligado ao usuário e ao security stamp.",
                "Montar ClaimsPrincipal para cookie ou bearer.",
                "Autorizar por role, claim de e-mail e nível de segurança.",
                "Escolher cookie, bearer ou external conforme endpoint e cliente.",
                "Emitir access_token com issuer, audience, expiração e claims esperadas.",
                "Conectar OAuth/OpenID por authorization code, callback e mapeamento de claims."
            ]);
    }
}
