using System.Security.Claims;

namespace CSharpCodePortfolio.Tutorials.Tutorial25;

internal sealed class PortfolioIdentityUser
{
    private readonly List<Claim> claims = [];
    private readonly List<string> roles = [];

    public PortfolioIdentityUser(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Id = Guid.NewGuid().ToString("N");
        Email = email;
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    public string Id { get; }

    public string Email { get; }

    public string UserName => Email;

    public string SecurityStamp { get; }

    public string? PasswordHash { get; set; }

    public bool EmailConfirmed { get; private set; }

    public IReadOnlyList<Claim> Claims => claims;

    public IReadOnlyList<string> Roles => roles;

    public void AddClaim(string type, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (claims.Any(claim => claim.Type == type && claim.Value == value))
        {
            return;
        }

        claims.Add(new Claim(type, value));
    }

    public void AddRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        if (!roles.Contains(role, StringComparer.Ordinal))
        {
            roles.Add(role);
        }
    }

    public bool TryConfirmEmail(IdentityEmailToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (!token.BelongsTo(this))
        {
            return false;
        }

        EmailConfirmed = true;
        return true;
    }
}
