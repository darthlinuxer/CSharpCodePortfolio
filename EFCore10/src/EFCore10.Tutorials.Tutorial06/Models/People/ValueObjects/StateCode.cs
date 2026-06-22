using System.Text.RegularExpressions;
using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record StateCode
{
    private StateCode(string value) => Value = value;

    public string Value { get; }

    public static StateCode Create(string value) => new(Validate(value));

    public override string ToString() => Value;

    private static string Validate(string? value)
    {
        var normalized = value.ToUpperInvariantRequired("State code");

        return Pattern().IsMatch(normalized) ? normalized : throw new DomainException("State code must have two letters.");
    }

    [GeneratedRegex(@"^[A-Z]{2}$")]
    private static partial Regex Pattern();
}
