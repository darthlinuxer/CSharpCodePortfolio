using System.Net.Mail;
using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record Email
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string value) => new(Normalize(value));

    public override string ToString() => Value;

    private static string Normalize(string? value)
    {
        var normalized = value.ToLowerInvariantRequired("Email");

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
