using System.Security.Claims;
using CSharpCodePortfolio.Tutorials.Tutorial25;
using Microsoft.AspNetCore.Identity;

namespace CSharpCodePortfolio.Tutorials.Tutorial25.Tests;

[TestClass]
public sealed class IdentityFrameworkScenarioTests
{
    [TestMethod]
    public void Run_ProducesIdentityFlowReport()
    {
        var report = IdentityFrameworkScenario.Run();

        Assert.AreEqual("ana.admin@portfolio.test", report.Email);
        Assert.IsTrue(report.PasswordHashLooksProtected);
        Assert.IsFalse(report.CanSignInBeforeEmailConfirmation);
        Assert.IsTrue(report.EmailConfirmationSucceeded);
        Assert.IsTrue(report.CanSignInAfterEmailConfirmation);
        Assert.AreEqual(PasswordVerificationResult.Success, report.CorrectPasswordResult);
        Assert.AreEqual(PasswordVerificationResult.Failed, report.WrongPasswordResult);
        Assert.AreEqual(IdentityConstants.ApplicationScheme, report.Claims.CookieAuthenticationType);
        Assert.AreEqual("Bearer", report.Claims.BearerAuthenticationType);
        Assert.IsTrue(report.Claims.HasRole);
        Assert.IsTrue(report.Claims.HasEmailClaim);
        Assert.IsTrue(report.Claims.HasSecurityLevelClaim);
        Assert.IsTrue(report.Claims.PrincipalMustBeRenewedAfterClaimChanges);
        Assert.AreEqual("CSharpCodePortfolio", report.Token.Issuer);
        Assert.AreEqual("portfolio-api", report.Token.Audience);
        Assert.AreEqual(TimeSpan.FromHours(8), report.Token.Lifetime);
        Assert.AreEqual(TimeSpan.Zero, report.Token.ClockSkew);
        Assert.IsTrue(report.Token.Claims.Contains("role=User"));
        Assert.IsTrue(report.Token.Claims.Contains("Email=ana.admin@portfolio.test"));
        Assert.IsTrue(report.Token.Claims.Contains("SecurityLevel=10"));
        Assert.IsTrue(report.Token.AuthenticationSchemes.Any(step => step.Contains(IdentityConstants.ApplicationScheme, StringComparison.Ordinal)));
        Assert.IsTrue(report.Token.AuthenticationSchemes.Any(step => step.Contains("Bearer", StringComparison.Ordinal)));
        Assert.IsTrue(report.Token.ExternalProviderSteps.Any(step => step.Contains("OpenID", StringComparison.Ordinal)));
        Assert.HasCount(9, report.FlowSteps);
    }

    [TestMethod]
    public void Register_HashesPasswordAndRequiresConfirmedEmail()
    {
        var flow = new IdentityPasswordFlow();

        var result = flow.Register(
            new IdentityRegistrationRequest("dev@portfolio.test", "senha-segura"));

        Assert.IsNotNull(result.User.PasswordHash);
        Assert.IsFalse(result.User.PasswordHash.Contains("senha-segura", StringComparison.Ordinal));
        Assert.IsFalse(result.CanSignInBeforeEmailConfirmation);
        Assert.IsTrue(result.EmailConfirmationSucceeded);
        Assert.IsTrue(result.User.EmailConfirmed);
        Assert.IsTrue(result.CanSignInAfterEmailConfirmation);
        Assert.AreEqual(PasswordVerificationResult.Success, flow.VerifyPassword(result.User, "senha-segura"));
        Assert.AreEqual(PasswordVerificationResult.Failed, flow.VerifyPassword(result.User, "senha-errada"));

        var otherUser = new PortfolioIdentityUser("outro@portfolio.test");
        Assert.IsFalse(otherUser.TryConfirmEmail(result.EmailConfirmationToken));
    }

    [TestMethod]
    public void BuildPrincipal_UsesRoleClaimTypeAndCustomClaims()
    {
        var user = new PortfolioIdentityUser("claim@portfolio.test");
        user.AddRole("User");
        user.AddClaim("Email", user.Email);
        user.AddClaim("SecurityLevel", "10");

        var principal = IdentityClaimsFlow.BuildPrincipal(user, IdentityConstants.ApplicationScheme);
        var userPolicy = IdentityClaimsFlow.CreateUserPolicy();
        var securityPolicy = IdentityClaimsFlow.CreateSecurityLevelPolicy(10);

        Assert.AreEqual(IdentityConstants.ApplicationScheme, principal.Identity?.AuthenticationType);
        Assert.IsTrue(principal.HasClaim(ClaimTypes.Role, "User"));
        Assert.IsTrue(principal.IsInRole("User"));
        Assert.IsTrue(principal.HasClaim("Email", "claim@portfolio.test"));
        Assert.IsTrue(principal.HasClaim("SecurityLevel", "10"));
        Assert.IsTrue(userPolicy.AuthenticationSchemes.Contains(IdentityConstants.ApplicationScheme));
        Assert.IsTrue(userPolicy.AuthenticationSchemes.Contains("Bearer"));
        Assert.IsTrue(userPolicy.Requirements.Count is >= 3);
        Assert.AreEqual("Bearer", securityPolicy.AuthenticationSchemes.Single());
        Assert.IsTrue(securityPolicy.Requirements.Count is >= 2);
    }

    [TestMethod]
    public void CreateTokenPlan_DescribesJwtValidationContract()
    {
        var registration = new IdentityPasswordFlow().Register(
            new IdentityRegistrationRequest("token@portfolio.test", "senha-segura"));

        var plan = IdentityTokenFlow.CreateTokenPlan(registration.User);

        Assert.AreEqual("CSharpCodePortfolio", plan.Issuer);
        Assert.AreEqual("portfolio-api", plan.Audience);
        Assert.AreEqual(TimeSpan.FromHours(8), plan.Lifetime);
        Assert.AreEqual(TimeSpan.Zero, plan.ClockSkew);
        Assert.IsTrue(plan.Claims.Contains("role=User"));
        Assert.IsTrue(plan.Claims.Contains("email=token@portfolio.test"));
        Assert.IsTrue(plan.Claims.Contains("Email=token@portfolio.test"));
        Assert.IsTrue(plan.AuthenticationSchemes.Any(step => step.Contains(IdentityConstants.ApplicationScheme, StringComparison.Ordinal)));
        Assert.IsTrue(plan.AuthenticationSchemes.Any(step => step.Contains("Bearer", StringComparison.Ordinal)));
        Assert.IsTrue(plan.ExternalProviderSteps.Any(step => step.Contains("PKCE", StringComparison.Ordinal)));
        Assert.IsTrue(plan.ExternalProviderSteps.Any(step => step.Contains("userinfo", StringComparison.Ordinal)));
        Assert.IsTrue(plan.ValidationSteps.Any(step => step.Contains("issuer", StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(plan.ValidationSteps.Any(step => step.Contains("access_token", StringComparison.Ordinal)));
        Assert.IsTrue(plan.ValidationSteps.Any(step => step.Contains("OnTokenValidated", StringComparison.Ordinal)));
    }
}
