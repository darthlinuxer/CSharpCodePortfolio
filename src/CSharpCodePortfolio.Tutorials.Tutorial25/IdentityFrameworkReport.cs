using Microsoft.AspNetCore.Identity;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal sealed record IdentityFrameworkReport(
    string Email,
    bool PasswordHashLooksProtected,
    bool CanSignInBeforeEmailConfirmation,
    bool CanSignInAfterEmailConfirmation,
    bool EmailConfirmationSucceeded,
    PasswordVerificationResult CorrectPasswordResult,
    PasswordVerificationResult WrongPasswordResult,
    IdentityClaimsReport Claims,
    JwtTokenPlan Token,
    IReadOnlyList<string> FlowSteps);
