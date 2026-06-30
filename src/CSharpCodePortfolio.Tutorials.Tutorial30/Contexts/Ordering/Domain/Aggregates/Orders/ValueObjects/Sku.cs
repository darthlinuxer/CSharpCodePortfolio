using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;

/// <summary>
/// Product reference captured by Ordering without modeling a ProductCatalog context yet.
/// </summary>
public readonly record struct Sku(string Value)
{
    public static Either<DomainError, Sku> Create(Option<string> value) =>
        value.Match(
            Some: text => string.IsNullOrWhiteSpace(text)
                ? Left<DomainError, Sku>(new SkuRequiredError())
                : Right<DomainError, Sku>(new Sku(text.Trim().ToUpperInvariant())),
            None: () => Left<DomainError, Sku>(new SkuRequiredError()));

    public override string ToString() => Value;
}

public sealed record SkuRequiredError()
    : DomainError(new DomainErrorCode("ordering.sku_required"), "SKU obrigatório.")
{
    public override DomainErrorCategory Category => DomainErrorCategory.Validation;
}
