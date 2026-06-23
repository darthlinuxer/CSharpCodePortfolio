namespace CSharpCodePortfolio.Tutorials.Tutorial27;

internal sealed record EmailMessage(
    EmailAddress From,
    EmailAddress To,
    string Subject,
    string Body)
{
    public static EmailMessage Create(EmailAddress from, EmailAddress to, string subject, string body)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new EmailMessage(from, to, subject.Trim(), body.Trim());
    }

    public string BodyPreview()
    {
        const int maxLength = 64;

        return Body.Length <= maxLength
            ? Body
            : $"{Body[..(maxLength - 3)]}...";
    }
}
