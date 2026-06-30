namespace CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

public sealed record DomainEventDispatchCycleLimitExceededError(int MaxCycles)
    : DomainError(
        new DomainErrorCode("persistence.domain_event_cycle_limit"),
        $"O limite de {MaxCycles} ciclos de eventos de domínio foi excedido.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}
