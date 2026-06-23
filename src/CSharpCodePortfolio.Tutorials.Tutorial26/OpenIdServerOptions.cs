namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal sealed record OpenIdServerOptions(
    string Issuer,
    string SigningKey,
    DateTimeOffset IssuedAt,
    TimeSpan CodeLifetime,
    TimeSpan TokenLifetime)
{
    public static OpenIdServerOptions TutorialDefaults()
    {
        return new OpenIdServerOptions(
            Issuer: "https://identity.local",
            SigningKey: "tutorial-signing-key-with-more-than-32-bytes",
            IssuedAt: new DateTimeOffset(2026, 6, 23, 12, 0, 0, TimeSpan.Zero),
            CodeLifetime: TimeSpan.FromMinutes(5),
            TokenLifetime: TimeSpan.FromMinutes(10));
    }
}
