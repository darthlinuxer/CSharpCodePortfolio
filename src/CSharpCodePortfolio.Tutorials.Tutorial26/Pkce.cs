using System.Security.Cryptography;
using System.Text;

namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal static class Pkce
{
    public static string CreateChallenge(string verifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(verifier);
        return Base64Url.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
    }
}
