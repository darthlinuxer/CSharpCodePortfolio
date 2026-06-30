using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;

public sealed record UserAccountNotFoundError()
    : DomainError(new DomainErrorCode("registration.user_not_found"), "Usuário não encontrado.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.NotFound;
}
