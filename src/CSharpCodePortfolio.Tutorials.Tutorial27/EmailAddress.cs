using System.Net.Mail;

namespace CSharpCodePortfolio.Tutorials.Tutorial27;

internal sealed record EmailAddress(string Value)
{
    public static EmailAddress Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim();
        try
        {
            var parsed = new MailAddress(trimmed);
            if (!string.Equals(parsed.Address, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("Use apenas o endereço de e-mail, sem nome de exibição.");
            }

            return new EmailAddress(parsed.Address);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException($"Endereço de e-mail inválido: {value}.", exception);
        }
    }

    public override string ToString()
    {
        return Value;
    }
}
