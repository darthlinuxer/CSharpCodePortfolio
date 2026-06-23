namespace CSharpCodePortfolio.Tutorials.Tutorial27;

internal sealed record GmailSmtpSendPlan(
    string Host,
    int Port,
    bool UsesStartTls,
    string UserName,
    string SecretName,
    string From,
    string To,
    string Subject,
    string BodyPreview,
    bool IsReadyToSend,
    IReadOnlyList<string> Issues);
