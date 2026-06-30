namespace CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

public sealed record PersistenceConflictError()
    : DomainError(new DomainErrorCode("persistence.conflict"), "A persistência rejeitou a alteração por conflito.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}
