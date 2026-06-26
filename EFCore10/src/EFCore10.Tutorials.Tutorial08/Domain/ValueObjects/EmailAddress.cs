using System.Net.Mail;

namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record EmailAddress
{
    private EmailAddress(string value) => Value = value;

    public const int MaxLength = 180;

    public string Value { get; }

    internal static EmailAddress Create(string? value)
    {
        var normalized = DomainText.Required(value, "Email", minLength: 3, MaxLength).ToLowerInvariant();

        try
        {
            var address = new MailAddress(normalized);

            return address.Address == normalized
                ? new EmailAddress(normalized)
                : throw new DomainException(DomainErrors.EmailInvalid, "Email is invalid.");
        }
        catch (FormatException exception)
        {
            throw new DomainException(DomainErrors.EmailInvalid, "Email is invalid.") { Source = exception.Source };
        }
    }

    internal static EmailAddress FromStorage(string value) => Create(value);
}
