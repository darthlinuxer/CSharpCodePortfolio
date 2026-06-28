using System.Net.Mail;

namespace EFCore10.Tutorials.Tutorial08.Domain.Common;

internal sealed record EmailAddress
{
    private EmailAddress(string value) => Value = value;

    public const int MaxLength = 180;

    public string Value { get; }

    internal static Result<EmailAddress> Create(string? value)
    {
        var text = DomainText.Required(value, "Email", minLength: 3, MaxLength);

        if (text.IsFailure)
            return Result<EmailAddress>.Failure(text.Errors);

        var normalized = text.RequireValue().ToLowerInvariant();

        return MailAddress.TryCreate(normalized, out var address) && address.Address == normalized
            ? Result<EmailAddress>.Success(new EmailAddress(normalized))
            : Result<EmailAddress>.Failure(DomainErrors.EmailInvalid);
    }

    internal static Result<EmailAddress> FromStorage(string value) => Create(value);
}
