namespace CSharpCodePortfolio.Tutorials.Tutorial21;

internal sealed record MassTransitDeliveryReport(
    PortfolioMessage Published,
    PortfolioMessage Consumed,
    int ConsumedMessages,
    string QueueName);
