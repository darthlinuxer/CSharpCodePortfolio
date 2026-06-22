using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogUrl
{
    private BlogUrl(string value) => Value = value;

    public string Value { get; }

    public static BlogUrl Create(string value) => new(Normalize(value));

    public override string ToString() => Value;

    private static string Normalize(string? value)
    {
        var normalized = value.TrimRequired("Blog URL");

        return Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https"
            ? normalized
            : throw new DomainException("Blog URL must be an absolute HTTP or HTTPS URL.");
    }
}
