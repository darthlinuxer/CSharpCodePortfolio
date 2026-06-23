namespace CSharpCodePortfolio.Tutorials.Tutorial27;

internal sealed record GmailSmtpEmailReport(
    GmailSmtpOptions Options,
    EmailMessage Message,
    GmailSmtpSendPlan SendPlan,
    bool InvalidDestinationBlocked,
    bool SenderMismatchBlocked,
    IReadOnlyList<string> Checklist);
