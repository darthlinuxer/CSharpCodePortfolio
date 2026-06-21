using System.Text.RegularExpressions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed partial record StateCode
{
    public StateCode(string value) => Value = value;

    public string Value
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("State code is required.");

            var normalized = value.Trim().ToUpperInvariant();
            field = Pattern().IsMatch(normalized) ? normalized : throw new DomainException("State code must have two letters.");
        }
    }

    public static StateCode Create(string value) => new(value);

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z]{2}$")]
    private static partial Regex Pattern();
}
