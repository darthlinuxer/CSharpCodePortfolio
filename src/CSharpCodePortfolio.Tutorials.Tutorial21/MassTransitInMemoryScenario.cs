using MassTransit;

namespace CSharpCodePortfolio.Tutorials.Tutorial21;

internal sealed class MassTransitInMemoryScenario
{
    private const string QueueName = "portfolio-messages";
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(5);

    public async Task<MassTransitDeliveryReport> RunAsync(CancellationToken cancellationToken)
    {
        var probe = new ReceivedMessageProbe();
        var bus = Bus.Factory.CreateUsingInMemory(cfg =>
        {
            cfg.ReceiveEndpoint(QueueName, endpoint =>
            {
                endpoint.Handler<PortfolioMessage>(context =>
                {
                    probe.Record(context.Message);
                    return Task.CompletedTask;
                });
            });
        });

        await bus.StartAsync(cancellationToken);

        try
        {
            var published = new PortfolioMessage(
                Guid.NewGuid(),
                "Mensagem publicada pelo tutorial MassTransit");

            await bus.Publish(published, cancellationToken);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(DeliveryTimeout);

            var consumed = await probe.WaitForMessageAsync(timeout.Token);
            return new MassTransitDeliveryReport(published, consumed, probe.ReceivedCount, QueueName);
        }
        finally
        {
            await bus.StopAsync(CancellationToken.None);
        }
    }
}
