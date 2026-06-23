namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record TokenValidationResult(
    bool IsValid,
    IReadOnlyDictionary<string, string> Claims,
    string? Error)
{
    public string? Subject => Claims.GetValueOrDefault("sub");

    public static TokenValidationResult Success(IReadOnlyDictionary<string, string> claims)
    {
        return new TokenValidationResult(true, claims, null);
    }

    public static TokenValidationResult Fail(string error)
    {
        return new TokenValidationResult(false, new Dictionary<string, string>(), error);
    }
}
