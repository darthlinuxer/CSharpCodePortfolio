namespace CSharpCodePortfolio.Tutorials.Tutorial27;

internal sealed record GmailSmtpOptions(
    string Host,
    int Port,
    bool UsesStartTls,
    EmailAddress UserName,
    string AppPasswordSecretName)
{
    public static GmailSmtpOptions TutorialDefaults()
    {
        return new GmailSmtpOptions(
            Host: "smtp.gmail.com",
            Port: 587,
            UsesStartTls: true,
            UserName: EmailAddress.Parse("remetente.portfolio@gmail.com"),
            AppPasswordSecretName: "GMAIL_APP_PASSWORD");
    }

    public IReadOnlyList<string> Validate()
    {
        var issues = new List<string>();
        if (!string.Equals(Host, "smtp.gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add("O host SMTP do Gmail deve ser smtp.gmail.com.");
        }

        if (Port != 587)
        {
            issues.Add("A porta recomendada para STARTTLS é 587.");
        }

        if (!UsesStartTls)
        {
            issues.Add("STARTTLS deve estar habilitado.");
        }

        if (string.IsNullOrWhiteSpace(AppPasswordSecretName))
        {
            issues.Add("Informe o nome do segredo que contém a senha de app ou token.");
        }

        return issues;
    }
}
