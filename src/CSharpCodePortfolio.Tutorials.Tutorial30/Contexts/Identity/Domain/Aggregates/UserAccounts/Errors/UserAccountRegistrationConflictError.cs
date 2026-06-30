using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;

public sealed record UserAccountRegistrationConflictError()
    : DomainError(new DomainErrorCode("registration.conflict"), "O cadastro conflita com outro usuário já persistido.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}
