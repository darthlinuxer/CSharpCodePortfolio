using Microsoft.AspNetCore.Identity;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal static class IdentitySecurityOptions
{
    public static IdentityOptions Create()
    {
        return new IdentityOptions
        {
            SignIn =
            {
                RequireConfirmedAccount = true
            },
            Password =
            {
                RequireLowercase = false,
                RequiredLength = 6,
                RequireDigit = false,
                RequireNonAlphanumeric = false,
                RequireUppercase = false
            },
            Lockout =
            {
                DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30),
                AllowedForNewUsers = true,
                MaxFailedAccessAttempts = 3
            },
            User =
            {
                RequireUniqueEmail = true
            }
        };
    }
}
