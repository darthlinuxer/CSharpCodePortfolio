namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Error returned when the email is already registered. Carries the
/// <see cref="DomainErrorCategory.Conflict"/> category so the presentation
/// layer can map it to HTTP 409 without pattern-matching the concrete type.
/// </summary>
/// <remarks>
/// Document-related errors have been removed alongside <c>Document</c>
/// (PF vs PJ modelling is deferred to a dedicated tutorial; the abstract
/// registration aggregate does not know about either).
/// </remarks>
public sealed record UserAccountEmailDuplicateError()
    : DomainError(new DomainErrorCode("registration.email_duplicate"), "Já existe usuário com esse email.")
{
    /// <inheritdoc />
    public override DomainErrorCategory Category => DomainErrorCategory.Conflict;
}