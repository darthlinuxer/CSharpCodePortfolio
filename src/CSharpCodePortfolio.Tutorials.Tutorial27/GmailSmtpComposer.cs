namespace CSharpCodePortfolio.Tutorials.Tutorial27;

internal sealed class GmailSmtpComposer(GmailSmtpOptions options)
{
    public GmailSmtpSendPlan Prepare(EmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var issues = options.Validate().ToList();
        if (!string.Equals(message.From.Value, options.UserName.Value, StringComparison.OrdinalIgnoreCase))
        {
            issues.Add("O remetente precisa ser a conta autenticada no Gmail.");
        }

        return new GmailSmtpSendPlan(
            Host: options.Host,
            Port: options.Port,
            UsesStartTls: options.UsesStartTls,
            UserName: options.UserName.Value,
            SecretName: options.AppPasswordSecretName,
            From: message.From.Value,
            To: message.To.Value,
            Subject: message.Subject,
            BodyPreview: message.BodyPreview(),
            IsReadyToSend: issues.Count == 0,
            Issues: issues);
    }
}
