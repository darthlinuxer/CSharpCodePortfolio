using CSharpCodePortfolio.Tutorials.Tutorial26;

namespace CSharpCodePortfolio.Tutorials.Tutorial26.Tests;

[TestClass]
public sealed class MinimumOpenIdServerScenarioTests
{
    [TestMethod]
    public void Run_ProducesOpenIdFlowReport()
    {
        var report = MinimumOpenIdServerScenario.Run();

        Assert.AreEqual("https://identity.local", report.Discovery.Issuer);
        Assert.AreEqual("https://identity.local/connect/authorize", report.Discovery.AuthorizationEndpoint);
        Assert.AreEqual("https://identity.local/connect/token", report.Discovery.TokenEndpoint);
        Assert.AreEqual("https://identity.local/connect/userinfo", report.Discovery.UserInfoEndpoint);
        Assert.IsTrue(report.Discovery.ResponseTypesSupported.Contains("code"));
        Assert.IsTrue(report.Discovery.ScopesSupported.Contains("openid"));
        Assert.AreEqual("state-123", report.Authorization.State);
        Assert.IsTrue(report.Authorization.RedirectUri.Contains("code=", StringComparison.Ordinal));
        Assert.IsTrue(report.Authorization.RedirectUri.Contains("state=state-123", StringComparison.Ordinal));
        Assert.AreEqual("Bearer", report.Token.TokenType);
        Assert.AreEqual("openid profile email", report.Token.Scope);
        Assert.IsTrue(report.IdTokenValidation.IsValid);
        Assert.IsTrue(report.AccessTokenValidation.IsValid);
        Assert.AreEqual("user-001", report.UserInfo.Subject);
        Assert.AreEqual("ana.admin@portfolio.test", report.UserInfo.Email);
        Assert.IsTrue(report.AuthorizationCodeReuseBlocked);
        Assert.IsTrue(report.TamperedAccessTokenBlocked);
        Assert.HasCount(7, report.FlowSteps);
    }

    [TestMethod]
    public void Exchange_RejectsWrongPkceVerifier()
    {
        var server = MinimumOpenIdServerScenario.CreateServer();
        var authorization = server.Authorize(new AuthorizationRequest(
            "portfolio-console",
            "https://client.local/callback",
            "openid profile email",
            "code",
            "state-123",
            Pkce.CreateChallenge("verifier-correto"),
            "S256"));

        Assert.ThrowsExactly<InvalidOperationException>(() => server.Exchange(new TokenRequest(
            "portfolio-console",
            "dev-secret",
            authorization.Code,
            "https://client.local/callback",
            "verifier-errado")));
    }

    [TestMethod]
    public void Exchange_RejectsCodeReuse()
    {
        var server = MinimumOpenIdServerScenario.CreateServer();
        var verifier = "portfolio-verifier-12345";
        var authorization = server.Authorize(new AuthorizationRequest(
            "portfolio-console",
            "https://client.local/callback",
            "openid profile email",
            "code",
            "state-123",
            Pkce.CreateChallenge(verifier),
            "S256"));

        _ = server.Exchange(new TokenRequest(
            "portfolio-console",
            "dev-secret",
            authorization.Code,
            "https://client.local/callback",
            verifier));

        Assert.ThrowsExactly<InvalidOperationException>(() => server.Exchange(new TokenRequest(
            "portfolio-console",
            "dev-secret",
            authorization.Code,
            "https://client.local/callback",
            verifier)));
    }

    [TestMethod]
    public void ValidateToken_RejectsTamperedToken()
    {
        var report = MinimumOpenIdServerScenario.Run();
        var validation = MinimumOpenIdServerScenario
            .CreateServer()
            .ValidateToken($"{report.Token.AccessToken}x", "access_token");

        Assert.IsFalse(validation.IsValid);
        Assert.AreEqual("Assinatura inválida.", validation.Error);
    }
}
