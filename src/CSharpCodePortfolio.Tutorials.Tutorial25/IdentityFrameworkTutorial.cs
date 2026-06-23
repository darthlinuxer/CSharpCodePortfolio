using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

[Tutorial("25", "identity-framework", "ASP.NET Identity com Claims e JWT")]
public sealed class IdentityFrameworkTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("25", "ASP.NET Identity com Claims e JWT");
        TutorialConsole.WriteContext(
            ("Tema", "ASP.NET Core Identity"),
            ("Conceito", "Hash de senha, confirmação de conta, claims, policies, JWT e OpenID Connect"),
            ("Runtime", ".NET 10"),
            ("Slug", "identity-framework"));
        TutorialConsole.WriteQuestion("Como demonstrar Identity sem carregar um app web inteiro para dentro do tutorial?");
        TutorialConsole.WriteHypothesis(
            "O `PasswordHasher<TUser>` prova o ponto de segurança da senha sem depender de banco ou controller.",
            "A confirmação de e-mail altera a decisão de login quando `RequireConfirmedAccount` está ativo.",
            "Claims e policies precisam escolher tipos consistentes; roles usam `ClaimTypes.Role` e claims customizadas ficam separadas.",
            "JWT deve carregar issuer, audience, expiração e claims suficientes para a API validar o acesso.",
            "OAuth/OpenID Connect precisa separar redirecionamento externo, callback e vínculo com o usuário local.");
        TutorialConsole.WritePreparation(
            "`IdentityPasswordFlow` usa tipos reais do ASP.NET Core Identity para hash e verificação.",
            "`IdentityClaimsFlow` monta principais para cookie e bearer sem hospedar middleware HTTP.",
            "`IdentityTokenFlow` descreve JWT, esquemas e provedores externos sem introduzir segredos ou dependência de assinatura JWT neste ciclo.");

        TutorialConsole.WriteExperiment(
            1,
            "Registro e confirmação",
            "Cria o usuário, gera hash de senha, adiciona claims iniciais e confirma o e-mail com token ligado ao security stamp.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o registro não grava senha em texto claro.",
            typeof(IdentityPasswordFlow),
            nameof(IdentityPasswordFlow.Register),
            new CodeExcerpt(7, 15, "Hash, claims iniciais e confirmação"));
        TutorialConsole.WriteCodeSnippet(
            "Código real: a conta confirmada participa da decisão de login.",
            typeof(IdentityPasswordFlow),
            nameof(IdentityPasswordFlow.CanSignIn),
            new CodeExcerpt(6, 12, "RequireConfirmedAccount"));

        TutorialConsole.WriteExperiment(
            2,
            "Claims e policies",
            "Monta `ClaimsPrincipal` para cookie ou bearer e cria policies por role, e-mail e nível de segurança.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: role padrão e claims customizadas ficam explícitas.",
            typeof(IdentityClaimsFlow),
            nameof(IdentityClaimsFlow.BuildPrincipal),
            new CodeExcerpt(6, 20, "ClaimsPrincipal"));
        TutorialConsole.WriteCodeSnippet(
            "Código real: a policy combina usuário autenticado, role e claim customizada.",
            typeof(IdentityClaimsFlow),
            nameof(IdentityClaimsFlow.CreateUserPolicy));

        TutorialConsole.WriteExperiment(
            3,
            "Contrato de autenticação",
            "Descreve os dados do access token, os esquemas da aplicação e o roteiro de integração com provedores OAuth/OpenID.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: issuer, audience, expiração, esquemas e recarga de claims ficam no contrato.",
            typeof(IdentityTokenFlow),
            nameof(IdentityTokenFlow.CreateTokenPlan),
            new CodeExcerpt(5, 34, "Plano de access_token"));

        var report = IdentityFrameworkScenario.Run();

        TutorialConsole.WriteEvidence(
            "Registro",
            ("E-mail", report.Email),
            ("Hash protegido", report.PasswordHashLooksProtected ? "Sim" : "Não"),
            ("Login antes da confirmação", report.CanSignInBeforeEmailConfirmation ? "Liberado" : "Bloqueado"),
            ("Confirmação de e-mail", report.EmailConfirmationSucceeded ? "Confirmada" : "Falhou"),
            ("Login depois da confirmação", report.CanSignInAfterEmailConfirmation ? "Liberado" : "Bloqueado"),
            ("Senha correta", report.CorrectPasswordResult.ToString()),
            ("Senha incorreta", report.WrongPasswordResult.ToString()));
        TutorialConsole.WriteEvidence(
            "Claims e policies",
            ("Esquema cookie", report.Claims.CookieAuthenticationType),
            ("Esquema bearer", report.Claims.BearerAuthenticationType),
            ("Role User", report.Claims.HasRole ? "Presente" : "Ausente"),
            ("Claim Email", report.Claims.HasEmailClaim ? "Presente" : "Ausente"),
            ("Claim SecurityLevel", report.Claims.HasSecurityLevelClaim ? "Presente" : "Ausente"),
            ("Requisitos UserPolicy", report.Claims.UserPolicyRequirementCount.ToString()),
            ("Requisitos SecurityPolicy", report.Claims.SecurityPolicyRequirementCount.ToString()),
            ("Renovar principal após claims", report.Claims.PrincipalMustBeRenewedAfterClaimChanges ? "Sim" : "Não"));
        TutorialConsole.WriteEvidence(
            "Token",
            ("Issuer", report.Token.Issuer),
            ("Audience", report.Token.Audience),
            ("Expiração", report.Token.Lifetime.ToString()),
            ("Clock skew", report.Token.ClockSkew.ToString()));
        TutorialConsole.WriteEvidence(
            "Esquemas de autenticação",
            report.Token.AuthenticationSchemes.Select((scheme, index) => ($"{index + 1:00}", scheme)).ToArray());
        TutorialConsole.WriteEvidence(
            "OAuth/OpenID",
            report.Token.ExternalProviderSteps.Select((step, index) => ($"{index + 1:00}", step)).ToArray());
        TutorialConsole.WriteEvidence(
            "Claims do access_token",
            report.Token.Claims.Select((claim, index) => ($"{index + 1:00}", claim)).ToArray());
        TutorialConsole.WriteEvidence(
            "Roteiro",
            report.FlowSteps.Select((step, index) => ($"{index + 1:00}", step)).ToArray());

        TutorialConsole.WriteExperiment(
            4,
            "Teste automatizado",
            "Os testes validam hash, confirmação, policies, esquemas e contrato do token sem servidor HTTP.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: os testes verificam o contrato que o tutorial ensina.",
            "IdentityFrameworkScenarioTests.cs",
            """
            Assert.IsFalse(report.CanSignInBeforeEmailConfirmation);
            Assert.IsTrue(report.EmailConfirmationSucceeded);
            Assert.IsTrue(report.Token.AuthenticationSchemes.Any(
                step => step.Contains(
                    IdentityConstants.ApplicationScheme,
                    StringComparison.Ordinal)));
            Assert.IsTrue(report.Token.ExternalProviderSteps.Any(
                step => step.Contains("OpenID", StringComparison.Ordinal)));
            Assert.HasCount(9, report.FlowSteps);
            """);

        TutorialConsole.WriteObservation(
            "Em produção, use `UserManager`, `SignInManager`, stores do Identity e token providers do framework; aqui o console isola o comportamento que precisa ser entendido.");
        TutorialConsole.WriteConclusion(
            "Identity fica mais claro quando senha, confirmação, claims, policies, tokens e provedores externos aparecem como contratos pequenos antes de entrarem no pipeline HTTP.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }
}
