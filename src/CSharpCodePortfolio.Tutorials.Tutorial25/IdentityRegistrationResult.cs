using Microsoft.AspNetCore.Identity;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal sealed record IdentityRegistrationResult(
    PortfolioIdentityUser User,
    IdentityEmailToken EmailConfirmationToken,
    bool CanSignInBeforeEmailConfirmation,
    bool EmailConfirmationSucceeded,
    bool CanSignInAfterEmailConfirmation,
    PasswordVerificationResult CorrectPasswordResult,
    PasswordVerificationResult WrongPasswordResult);
