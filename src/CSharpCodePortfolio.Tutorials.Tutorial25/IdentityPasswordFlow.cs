using Microsoft.AspNetCore.Identity;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal sealed class IdentityPasswordFlow
{
    private readonly PasswordHasher<PortfolioIdentityUser> passwordHasher = new();

    public IdentityRegistrationResult Register(IdentityRegistrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);

        var user = new PortfolioIdentityUser(request.Email);
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        user.AddRole("User");
        user.AddClaim("Email", user.Email);
        user.AddClaim("SecurityLevel", "10");

        var token = IdentityEmailToken.CreateFor(user);
        var canSignInBeforeEmailConfirmation = CanSignIn(user, request.Password);
        var emailConfirmationSucceeded = user.TryConfirmEmail(token);

        return new IdentityRegistrationResult(
            user,
            token,
            canSignInBeforeEmailConfirmation,
            emailConfirmationSucceeded,
            CanSignIn(user, request.Password),
            VerifyPassword(user, request.Password),
            VerifyPassword(user, "senha-incorreta"));
    }

    public bool CanSignIn(PortfolioIdentityUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var options = IdentitySecurityOptions.Create();
        if (options.SignIn.RequireConfirmedAccount && !user.EmailConfirmed)
        {
            return false;
        }

        return VerifyPassword(user, password) != PasswordVerificationResult.Failed;
    }

    public PasswordVerificationResult VerifyPassword(PortfolioIdentityUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return PasswordVerificationResult.Failed;
        }

        return passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
    }
}
