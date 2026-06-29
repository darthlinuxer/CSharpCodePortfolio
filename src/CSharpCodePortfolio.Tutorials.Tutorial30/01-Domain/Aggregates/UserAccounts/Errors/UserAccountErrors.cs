namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Error returned when the required user document is missing or malformed.
/// </summary>
/// <remarks>
/// Task 4 of the execution plan eliminates the document concept entirely
/// (PF vs PJ modeling is deferred). The error type remains in the codebase
/// only because <see cref="UserAccount.NormalizeDocument"/> and the tests
/// still reference it. Task 4 will remove this error type alongside
/// <see cref="Document"/>.
/// </remarks>
public sealed record UserAccountDocumentInvalidError()
    : DomainError(new DomainErrorCode("registration.document_invalid"), "Documento obrigatório ou inválido.")
{
    /// <inheritdoc />
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}

/// <summary>
/// Error returned when the document is already registered.
/// </summary>
/// <remarks>
/// See remarks on <see cref="UserAccountDocumentInvalidError"/> — Task 4
/// will remove this type entirely.
/// </remarks>
public sealed record UserAccountDocumentDuplicateError()
    : DomainError(new DomainErrorCode("registration.document_duplicate"), "Já existe usuário com esse documento.")
{
    /// <inheritdoc />
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}

/// <summary>
/// Error returned when the email is already registered. Carries the
/// <see cref="DomainErrorCategory.Conflict"/> category so the presentation
/// layer can map it to HTTP 409 without pattern-matching the concrete type.
/// </summary>
public sealed record UserAccountEmailDuplicateError()
    : DomainError(new DomainErrorCode("registration.email_duplicate"), "Já existe usuário com esse email.")
{
    /// <inheritdoc />
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}