namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Catalog of expected registration failures used by domain factories and application orchestration.
/// </summary>
public static class DomainErrors
{
    /// <summary>
    /// Error returned when the required user name is missing.
    /// </summary>
    public static readonly DomainError NameRequired =
        new("registration.name_required", "Nome obrigatório.");

    /// <summary>
    /// Error returned when the required document is missing or malformed.
    /// </summary>
    public static readonly DomainError DocumentInvalid =
        new("registration.document_invalid", "Documento obrigatório ou inválido.");

    /// <summary>
    /// Error returned when the required email is missing or malformed.
    /// </summary>
    public static readonly DomainError EmailInvalid =
        new("registration.email_invalid", "Email informado é inválido.");

    /// <summary>
    /// Error returned when an optional phone was supplied but is malformed.
    /// </summary>
    public static readonly DomainError PhoneNumberInvalid =
        new("registration.phone_invalid", "Telefone informado é inválido.");

    /// <summary>
    /// Error returned when the document already exists.
    /// </summary>
    public static readonly DomainError DocumentDuplicate =
        new("registration.document_duplicate", "Já existe usuário com esse documento.");

    /// <summary>
    /// Error returned when the email already exists.
    /// </summary>
    public static readonly DomainError EmailDuplicate =
        new("registration.email_duplicate", "Já existe usuário com esse email.");

    /// <summary>
    /// Error returned when a domain timestamp is not UTC.
    /// </summary>
    public static readonly DomainError TimestampUtcRequired =
        new("registration.timestamp_utc_required", "Timestamp de domínio deve estar em UTC.");
}
