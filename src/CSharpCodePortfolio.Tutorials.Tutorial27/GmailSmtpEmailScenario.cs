namespace CSharpCodePortfolio.Tutorials.Tutorial27;

internal static class GmailSmtpEmailScenario
{
    public static GmailSmtpEmailReport Run()
    {
        var options = GmailSmtpOptions.TutorialDefaults();
        var message = EmailMessage.Create(
            from: options.UserName,
            to: EmailAddress.Parse("destino.portfolio@gmail.com"),
            subject: "Resumo do portfolio",
            body: "O tutorial preparou uma mensagem SMTP sem expor credenciais.");
        var composer = new GmailSmtpComposer(options);
        var sendPlan = composer.Prepare(message);
        var invalidDestinationBlocked = ThrowsInvalidOperation(() => EmailAddress.Parse("destino-invalido"));
        var senderMismatchBlocked = !composer.Prepare(message with
        {
            From = EmailAddress.Parse("outra.conta@gmail.com")
        }).IsReadyToSend;

        return new GmailSmtpEmailReport(
            options,
            message,
            sendPlan,
            invalidDestinationBlocked,
            senderMismatchBlocked,
            Checklist:
            [
                "Validar os endereços antes de montar a mensagem.",
                "Usar smtp.gmail.com na porta 587 com STARTTLS.",
                "Autenticar com a conta remetente.",
                "Ler o segredo de User Secrets, Key Vault ou variável de ambiente.",
                "Enviar apenas depois que destinatário, remetente, assunto e corpo estiverem válidos."
            ]);
    }

    private static bool ThrowsInvalidOperation(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }
}
