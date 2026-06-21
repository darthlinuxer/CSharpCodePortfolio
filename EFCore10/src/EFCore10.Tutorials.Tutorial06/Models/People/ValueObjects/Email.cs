using System.Net.Mail;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record Email
{
    public Email(string value) => Value = value;

    public string Value
    {
        get;
        init => field = Normalize(value);
    }

    public static Email Create(string value) => new(value);

    public override string ToString() => Value;

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email is required.");

        var normalized = value.Trim().ToLowerInvariant();

        try
        {
            var address = new MailAddress(normalized);
            return address.Address == normalized
                ? normalized
                : throw new DomainException("Email is invalid.");
        }
        catch (FormatException exception)
        {
            throw new DomainException("Email is invalid.") { Source = exception.Source };
        }
    }
}
