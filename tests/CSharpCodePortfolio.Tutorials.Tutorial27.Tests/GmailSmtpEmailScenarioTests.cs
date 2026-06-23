using CSharpCodePortfolio.Tutorials.Tutorial27;

namespace CSharpCodePortfolio.Tutorials.Tutorial27.Tests;

[TestClass]
public sealed class GmailSmtpEmailScenarioTests
{
    [TestMethod]
    public void Run_ProducesGmailSmtpSendPlan()
    {
        var report = GmailSmtpEmailScenario.Run();

        Assert.AreEqual("smtp.gmail.com", report.SendPlan.Host);
        Assert.AreEqual(587, report.SendPlan.Port);
        Assert.IsTrue(report.SendPlan.UsesStartTls);
        Assert.AreEqual("remetente.portfolio@gmail.com", report.SendPlan.UserName);
        Assert.AreEqual("GMAIL_APP_PASSWORD", report.SendPlan.SecretName);
        Assert.AreEqual("destino.portfolio@gmail.com", report.SendPlan.To);
        Assert.IsTrue(report.SendPlan.IsReadyToSend);
        Assert.IsEmpty(report.SendPlan.Issues);
        Assert.IsTrue(report.InvalidDestinationBlocked);
        Assert.IsTrue(report.SenderMismatchBlocked);
        Assert.HasCount(5, report.Checklist);
    }

    [TestMethod]
    public void EmailAddress_ParseRejectsInvalidAddress()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => EmailAddress.Parse("destino-invalido"));

        Assert.StartsWith("Endereço de e-mail inválido", exception.Message);
    }

    [TestMethod]
    public void Prepare_FlagsSenderMismatch()
    {
        var options = GmailSmtpOptions.TutorialDefaults();
        var plan = new GmailSmtpComposer(options).Prepare(EmailMessage.Create(
            from: EmailAddress.Parse("outra.conta@gmail.com"),
            to: EmailAddress.Parse("destino.portfolio@gmail.com"),
            subject: "Resumo do portfolio",
            body: "Mensagem válida."));

        Assert.IsFalse(plan.IsReadyToSend);
        Assert.Contains("O remetente precisa ser a conta autenticada no Gmail.", plan.Issues);
    }

    [TestMethod]
    public void EmailMessage_CreateRejectsBlankSubject()
    {
        Assert.ThrowsExactly<ArgumentException>(() => EmailMessage.Create(
            from: EmailAddress.Parse("remetente.portfolio@gmail.com"),
            to: EmailAddress.Parse("destino.portfolio@gmail.com"),
            subject: " ",
            body: "Mensagem válida."));
    }
}
