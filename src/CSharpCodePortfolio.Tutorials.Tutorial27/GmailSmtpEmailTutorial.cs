using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial27;

[Tutorial("27", "gmail-smtp-email", "Envio de e-mail com Gmail SMTP")]
public sealed class GmailSmtpEmailTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("27", "Envio de e-mail com Gmail SMTP");
        TutorialConsole.WriteContext(
            ("Tema", "SMTP"),
            ("Conceito", "Mensagem, destinatário, remetente autenticado, STARTTLS e segredo externo"),
            ("Runtime", ".NET 10"),
            ("Slug", "gmail-smtp-email"));
        TutorialConsole.WriteQuestion("Como preparar um envio de e-mail pelo Gmail sem expor credenciais?");
        TutorialConsole.WriteHypothesis(
            "O destinatário deve ser validado antes de montar a mensagem.",
            "O Gmail exige a conta autenticada como remetente efetivo do envio.",
            "A configuração SMTP fica fixa no host smtp.gmail.com, porta 587 e STARTTLS.",
            "A senha de app ou token fica fora do código, referenciada por nome de segredo.");
        TutorialConsole.WritePreparation(
            "`EmailAddress` usa `System.Net.Mail.MailAddress` para validar o formato do e-mail.",
            "`GmailSmtpOptions` guarda host, porta, STARTTLS, conta e nome do segredo sem armazenar a senha.",
            "`GmailSmtpComposer` prepara o plano de envio em memória; o tutorial não abre conexão SMTP.");

        TutorialConsole.WriteExperiment(
            1,
            "Endereço e mensagem",
            "Valida remetente, destinatário, assunto e corpo antes de qualquer tentativa de envio.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: a validação usa o parser de e-mail da biblioteca padrão.",
            typeof(EmailAddress),
            nameof(EmailAddress.Parse));
        TutorialConsole.WriteCodeSnippet(
            "Código real: a mensagem não aceita assunto ou corpo vazios.",
            typeof(EmailMessage),
            nameof(EmailMessage.Create));

        TutorialConsole.WriteExperiment(
            2,
            "Configuração SMTP",
            "Centraliza as constantes do Gmail e referencia o segredo pelo nome usado na configuração externa.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o tutorial usa smtp.gmail.com, porta 587 e STARTTLS.",
            typeof(GmailSmtpOptions),
            nameof(GmailSmtpOptions.TutorialDefaults));

        TutorialConsole.WriteExperiment(
            3,
            "Plano de envio",
            "Monta o plano final e bloqueia remetente diferente da conta autenticada.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o plano lista pendências em vez de enviar uma mensagem insegura.",
            typeof(GmailSmtpComposer),
            nameof(GmailSmtpComposer.Prepare));

        var report = GmailSmtpEmailScenario.Run();

        TutorialConsole.WriteEvidence(
            "SMTP",
            ("Host", report.SendPlan.Host),
            ("Porta", report.SendPlan.Port.ToString()),
            ("STARTTLS", report.SendPlan.UsesStartTls ? "Sim" : "Não"),
            ("Conta", report.SendPlan.UserName),
            ("Segredo", report.SendPlan.SecretName));
        TutorialConsole.WriteEvidence(
            "Mensagem",
            ("De", report.SendPlan.From),
            ("Para", report.SendPlan.To),
            ("Assunto", report.SendPlan.Subject),
            ("Prévia do corpo", report.SendPlan.BodyPreview),
            ("Pronta para envio", report.SendPlan.IsReadyToSend ? "Sim" : "Não"));
        TutorialConsole.WriteEvidence(
            "Proteções",
            ("Destino inválido bloqueado", report.InvalidDestinationBlocked ? "Sim" : "Não"),
            ("Remetente divergente bloqueado", report.SenderMismatchBlocked ? "Sim" : "Não"),
            ("Pendências", report.SendPlan.Issues.Count == 0 ? "Nenhuma" : string.Join("; ", report.SendPlan.Issues)));
        TutorialConsole.WriteEvidence(
            "Checklist",
            report.Checklist.Select((item, index) => ($"{index + 1:00}", item)).ToArray());

        TutorialConsole.WriteExperiment(
            4,
            "Teste automatizado",
            "Os testes validam configuração SMTP, endereços, mensagem e bloqueio de remetente divergente.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: os testes travam o contrato didático do envio.",
            "tests/CSharpCodePortfolio.Tutorials.Tutorial27.Tests/GmailSmtpEmailScenarioTests.cs");

        TutorialConsole.WriteObservation(
            "A execução prepara o envio sem tocar na rede. Em produção, o envio acontece somente depois de carregar o segredo por configuração segura e registrar falhas de SMTP sem imprimir credenciais.");
        TutorialConsole.WriteConclusion(
            "Um envio de e-mail pelo Gmail fica claro quando a mensagem, a conta remetente, o STARTTLS e o segredo externo aparecem como partes separadas do fluxo.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }
}
