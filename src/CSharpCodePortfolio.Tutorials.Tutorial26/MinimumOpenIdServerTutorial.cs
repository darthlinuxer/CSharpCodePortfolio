using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial26;

[Tutorial("26", "minimum-openid-server", "Servidor OpenID mínimo")]
public sealed class MinimumOpenIdServerTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("26", "Servidor OpenID mínimo");
        TutorialConsole.WriteContext(
            ("Tema", "OpenID Connect"),
            ("Conceito", "Discovery, authorization code, PKCE, token endpoint, userinfo e validação HMAC"),
            ("Runtime", ".NET 10"),
            ("Slug", "minimum-openid-server"));
        TutorialConsole.WriteQuestion("Qual é o menor contrato útil para entender um provedor OpenID?");
        TutorialConsole.WriteHypothesis(
            "Discovery informa onde o cliente encontra autorização, token e userinfo.",
            "O endpoint de autorização emite apenas um authorization code vinculado ao cliente, ao redirect URI e ao PKCE.",
            "O endpoint de token troca o code por id_token e access_token somente uma vez.",
            "UserInfo depende de access_token válido; id_token autentica o usuário no cliente.");
        TutorialConsole.WritePreparation(
            "`MinimumOpenIdServer` mantém apenas um cliente, um usuário e uma tabela de authorization codes em memória.",
            "`HmacTokenService` cria tokens no formato JWT usando HMAC-SHA256 e valida issuer, assinatura, expiração e uso do token.",
            "Os segredos são valores didáticos fixos; em produção eles pertencem a `User Secrets`, Key Vault ou variável de ambiente.");

        TutorialConsole.WriteExperiment(
            1,
            "Discovery",
            "Publica os endpoints que um cliente OpenID precisa descobrir antes de iniciar o fluxo.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o discovery nasce do issuer e enumera os endpoints suportados.",
            typeof(MinimumOpenIdServer),
            nameof(MinimumOpenIdServer.GetDiscoveryDocument));

        TutorialConsole.WriteExperiment(
            2,
            "Authorization code",
            "Valida cliente, redirect URI, response_type, escopo e PKCE antes de redirecionar com code e state.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o authorization code fica vinculado ao cliente, redirect URI, usuário e code challenge.",
            typeof(MinimumOpenIdServer),
            nameof(MinimumOpenIdServer.Authorize));

        TutorialConsole.WriteExperiment(
            3,
            "Token e userinfo",
            "Troca o code por tokens, bloqueia reutilização e usa o access_token para ler userinfo.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o token endpoint valida segredo, code, redirect URI e PKCE antes de emitir tokens.",
            typeof(MinimumOpenIdServer),
            nameof(MinimumOpenIdServer.Exchange));
        TutorialConsole.WriteCodeSnippet(
            "Código real: userinfo exige access_token válido.",
            typeof(MinimumOpenIdServer),
            nameof(MinimumOpenIdServer.GetUserInfo));

        var report = MinimumOpenIdServerScenario.Run();

        TutorialConsole.WriteEvidence(
            "Discovery",
            ("Issuer", report.Discovery.Issuer),
            ("Authorization", report.Discovery.AuthorizationEndpoint),
            ("Token", report.Discovery.TokenEndpoint),
            ("UserInfo", report.Discovery.UserInfoEndpoint),
            ("Response types", string.Join(", ", report.Discovery.ResponseTypesSupported)),
            ("Scopes", string.Join(", ", report.Discovery.ScopesSupported)));
        TutorialConsole.WriteEvidence(
            "Authorization code",
            ("State preservado", report.Authorization.State),
            ("Redirect contém code", report.Authorization.RedirectUri.Contains("code=", StringComparison.Ordinal) ? "Sim" : "Não"),
            ("Redirect contém state", report.Authorization.RedirectUri.Contains("state=state-123", StringComparison.Ordinal) ? "Sim" : "Não"),
            ("Reuso bloqueado", report.AuthorizationCodeReuseBlocked ? "Sim" : "Não"));
        TutorialConsole.WriteEvidence(
            "Tokens",
            ("Tipo", report.Token.TokenType),
            ("Escopo", report.Token.Scope),
            ("Expira em", $"{report.Token.ExpiresIn} segundos"),
            ("id_token válido", report.IdTokenValidation.IsValid ? "Sim" : "Não"),
            ("access_token válido", report.AccessTokenValidation.IsValid ? "Sim" : "Não"),
            ("Assinatura adulterada bloqueada", report.TamperedAccessTokenBlocked ? "Sim" : "Não"));
        TutorialConsole.WriteEvidence(
            "UserInfo",
            ("Sub", report.UserInfo.Subject),
            ("E-mail", report.UserInfo.Email),
            ("Nome", report.UserInfo.Name));
        TutorialConsole.WriteEvidence(
            "Roteiro",
            report.FlowSteps.Select((step, index) => ($"{index + 1:00}", step)).ToArray());

        TutorialConsole.WriteExperiment(
            4,
            "Teste automatizado",
            "Os testes validam discovery, PKCE, uso único do code, assinatura HMAC e userinfo.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: os testes travam o contrato principal do fluxo.",
            "MinimumOpenIdServerScenarioTests.cs",
            """
            Assert.AreEqual("https://identity.local", report.Discovery.Issuer);
            Assert.IsTrue(report.AuthorizationCodeReuseBlocked);
            Assert.IsTrue(report.IdTokenValidation.IsValid);
            Assert.IsTrue(report.AccessTokenValidation.IsValid);
            Assert.AreEqual("ana.admin@portfolio.test", report.UserInfo.Email);
            Assert.IsTrue(report.TamperedAccessTokenBlocked);
            """);

        TutorialConsole.WriteObservation(
            "Este tutorial ensina o contrato do fluxo. Um provedor real ainda precisa de HTTPS, armazenamento persistente, rotação de chaves, consentimento e proteção contra replay distribuído.");
        TutorialConsole.WriteConclusion(
            "Um servidor OpenID mínimo é a composição de discovery, code de uso único, PKCE, emissão de tokens assinados e userinfo protegido por access_token.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }
}
