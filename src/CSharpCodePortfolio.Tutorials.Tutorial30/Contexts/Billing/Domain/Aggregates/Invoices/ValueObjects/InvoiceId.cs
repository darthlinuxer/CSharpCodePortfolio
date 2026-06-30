using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;

/// <summary>
/// Strongly typed invoice identity.
/// </summary>
public readonly record struct InvoiceId(Guid Value)
{
    public static InvoiceId New() => new(Guid.CreateVersion7());

    public static Either<DomainError, InvoiceId> Create(Guid value) =>
        value == Guid.Empty
            ? Left<DomainError, InvoiceId>(new InvoiceIdRequiredError())
            : Right<DomainError, InvoiceId>(new InvoiceId(value));

    public override string ToString() => Value.ToString();
}

public sealed record InvoiceIdRequiredError()
    : DomainError(new DomainErrorCode("billing.invoice_required"), "Fatura obrigatória.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
