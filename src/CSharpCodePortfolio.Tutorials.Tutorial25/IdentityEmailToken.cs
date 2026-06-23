using System.Security.Cryptography;
using System.Text;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal sealed record IdentityEmailToken(string UserId, string Purpose, string Value)
{
    public static IdentityEmailToken CreateFor(PortfolioIdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new IdentityEmailToken(user.Id, "ConfirmEmail", CreateValue(user));
    }

    public bool BelongsTo(PortfolioIdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return UserId == user.Id
            && Purpose == "ConfirmEmail"
            && Value == CreateValue(user);
    }

    private static string CreateValue(PortfolioIdentityUser user)
    {
        // ponytail: this mirrors the token boundary without hosting Identity's DataProtection stack; use UserManager token providers in real apps.
        var bytes = Encoding.UTF8.GetBytes($"{user.Id}:{user.SecurityStamp}:ConfirmEmail");
        return Convert.ToBase64String(SHA256.HashData(bytes));
    }
}
