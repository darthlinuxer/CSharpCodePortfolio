using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;

/// <summary>
/// Error returned when the email is already registered.
/// </summary>
public sealed record UserAccountEmailDuplicateError()
    : DomainError(new DomainErrorCode("registration.email_duplicate"), "Já existe usuário com esse email.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}
