using System.Text.RegularExpressions;
using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record UserName
{
    private UserName(string value) => Value = value;

    public string Value { get; }

    public static UserName Create(string value) => new(Validate(value));

    public override string ToString() => Value;

    private static string Validate(string? value)
    {
        var normalized = value.ToLowerInvariantRequired("User name");

        return Pattern().IsMatch(normalized)
            ? normalized
            : throw new DomainException("User name must have 3 to 50 URL-safe characters.");
    }

    [GeneratedRegex(@"^[a-z0-9._-]{3,50}$")]
    private static partial Regex Pattern();
}
