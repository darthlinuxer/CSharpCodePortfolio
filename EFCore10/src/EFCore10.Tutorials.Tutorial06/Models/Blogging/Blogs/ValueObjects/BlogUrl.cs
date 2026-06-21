namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record BlogUrl
{
    public BlogUrl(string value) => Value = value;

    public string Value
    {
        get;
        init => field = Normalize(value);
    }

    public static BlogUrl Create(string value) => new(value);

    public override string ToString() => Value;

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Blog URL is required.");

        var normalized = value.Trim();

        return Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https"
            ? normalized
            : throw new DomainException("Blog URL must be an absolute HTTP or HTTPS URL.");
    }
}
