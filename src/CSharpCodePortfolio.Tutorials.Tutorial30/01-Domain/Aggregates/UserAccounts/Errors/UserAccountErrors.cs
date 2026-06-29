using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Errors;

/// <summary>
/// Error returned when the required user document is missing or malformed.
/// </summary>
public sealed record UserAccountDocumentInvalidError()
    : DomainError(new DomainErrorCode("registration.document_invalid"), "Documento obrigatório ou inválido.");

/// <summary>
/// Error returned when the document is already registered.
/// </summary>
public sealed record UserAccountDocumentDuplicateError()
    : DomainError(new DomainErrorCode("registration.document_duplicate"), "Já existe usuário com esse documento.");

/// <summary>
/// Error returned when the email is already registered.
/// </summary>
public sealed record UserAccountEmailDuplicateError()
    : DomainError(new DomainErrorCode("registration.email_duplicate"), "Já existe usuário com esse email.");

/// <summary>
/// Error returned when persistence reports a registration conflict that cannot be narrowed further.
/// </summary>
public sealed record UserAccountRegistrationConflictError()
    : DomainError(new DomainErrorCode("registration.conflict"), "O cadastro conflita com outro usuário já persistido.");
