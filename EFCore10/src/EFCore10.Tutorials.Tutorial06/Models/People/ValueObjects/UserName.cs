using System.Text.RegularExpressions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record UserName
{
    public UserName(string value) => Value = value;

    public string Value
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("User name is required.");

            var normalized = value.Trim().ToLowerInvariant();
            field = Pattern().IsMatch(normalized)
                ? normalized
                : throw new DomainException("User name must have 3 to 50 URL-safe characters.");
        }
    }

    public static UserName Create(string value) => new(value);

    public override string ToString() => Value;

    [GeneratedRegex(@"^[a-z0-9._-]{3,50}$")]
    private static partial Regex Pattern();
}
