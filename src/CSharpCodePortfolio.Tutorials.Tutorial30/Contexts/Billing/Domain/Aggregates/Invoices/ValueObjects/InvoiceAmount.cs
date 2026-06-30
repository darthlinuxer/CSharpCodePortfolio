using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Billing.Domain.Aggregates.Invoices.ValueObjects;

/// <summary>
/// Billing-owned amount value object.
/// </summary>
public readonly record struct InvoiceAmount(decimal Value)
{
    public static Either<DomainError, InvoiceAmount> Create(decimal value) =>
        value <= 0
            ? Left<DomainError, InvoiceAmount>(new InvoiceAmountPositiveRequiredError())
            : Right<DomainError, InvoiceAmount>(new InvoiceAmount(decimal.Round(value, 2, MidpointRounding.AwayFromZero)));
}

public sealed record InvoiceAmountPositiveRequiredError()
    : DomainError(new DomainErrorCode("billing.amount_positive_required"), "Valor da fatura deve ser positivo.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
