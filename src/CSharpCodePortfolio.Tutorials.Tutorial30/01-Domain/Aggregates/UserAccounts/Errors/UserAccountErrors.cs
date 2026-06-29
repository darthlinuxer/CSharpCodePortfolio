using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Errors;

/// <summary>
/// Error returned when the email is already registered.
/// </summary>
public sealed record UserAccountEmailDuplicateError()
    : DomainError(new DomainErrorCode("registration.email_duplicate"), "Já existe usuário com esse email.")
{
    /// <inheritdoc />
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}

/// <summary>
/// Error returned when persistence reports a registration conflict that cannot be narrowed further.
/// </summary>
public sealed record UserAccountRegistrationConflictError()
    : DomainError(new DomainErrorCode("registration.conflict"), "O cadastro conflita com outro usuário já persistido.")
{
    /// <inheritdoc />
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}
